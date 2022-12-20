using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Analyzers.BooleanLiteral;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
#pragma warning disable CA1303
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LinqEnumerableAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesRule,
			Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesRule,
			Rules.DontConcatTwoCollectionsDefinedWithLiteralsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(c => CompilationStart(c));
		}

		public static bool HasEnumerableAppendMethod(Compilation compilation)
		{
			var enumerable = compilation.GetTypeByMetadataName("System.Linq.Enumerable");

			if (enumerable != null)
			{
				foreach (var symbol in enumerable.GetMembers(nameof(Enumerable.Append)))
				{
					if (symbol.Kind != SymbolKind.Method)
					{
						continue;
					}

					return true;
				}
			}

			return false;
		}

		public static void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp4))
			{
				return;
			}

			var hasEnumerableAppendMethod = HasEnumerableAppendMethod(context.Compilation);
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(c => Analyze(c, cache, hasEnumerableAppendMethod), SyntaxKind.SimpleMemberAccessExpression);
		}

		static bool ContainsSingleElement (ExpressionSyntax? e)
		{
			switch (e?.Kind())
			{
				case SyntaxKind.ImplicitArrayCreationExpression:
					return ((ImplicitArrayCreationExpressionSyntax)e)?.Initializer?.Expressions.Count == 1;
				case SyntaxKind.ObjectCreationExpression:
					return ((ObjectCreationExpressionSyntax)e)?.Initializer?.Expressions.Count == 1;
				case SyntaxKind.ArrayCreationExpression:
					return ((ArrayCreationExpressionSyntax)e)?.Initializer?.Expressions.Count == 1;
				default:
					return false;
			}
		}

		public static bool LooksLikeAppend(InvocationExpressionSyntax invocation) => (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[1].Expression));

		public static bool LooksLikePrepend(InvocationExpressionSyntax invocation)
		{
			var expression = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;

			if (expression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				expression = ((ParenthesizedExpressionSyntax)expression).GetExpression();
			}

			return (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression));
		}

		public static bool IsList(SemanticModel model, ObjectCreationExpressionSyntax e) => e.Type.ToString().StartsWith("List", StringComparison.Ordinal) && model.GetTypeInfo(e).Type?.MetadataName == "List`1";
		public static bool IsIEnumerable(SemanticModel model, ExpressionSyntax e) => model.GetTypeInfo(e).Type?.MetadataName == "IEnumerable`1";

		public static void Analyze (SyntaxNodeAnalysisContext context, FileDetailCache cache, bool hasEnumerable)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken) || !hasEnumerable)
			{
				return;
			}

			var expression = (MemberAccessExpressionSyntax)context.Node;

			if (expression != null)
			{
				if (!string.Equals(expression.Name.Identifier.Text, "Concat", StringComparison.Ordinal))
				{
					return;
				}

				if (!expression.Parent.IsKind(SyntaxKind.InvocationExpression))
				{
					return;
				}

				var invocation = (InvocationExpressionSyntax)expression.Parent;

				if (invocation == null)
				{
					return;
				}

				if (invocation.ArgumentList.Arguments.Count > 2 ||
					invocation.ArgumentList.Arguments.Count < 1)
				{
					return;
				}

				var semanticModel = context.SemanticModel;
				var arguments = invocation.ArgumentList.Arguments;

				if (LooksLikePrepend(invocation))
				{
					if (LooksLikeAppend(invocation))
					{
						foreach (var argument in arguments)
						{
							if (argument.IsKind(SyntaxKind.ObjectCreationExpression) && !IsList(semanticModel, (ObjectCreationExpressionSyntax)argument.Expression))
							{
								return;
							}
						}

						var e = expression.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)expression.Expression).GetExpression() : expression.Expression;

						if (e.IsKind(SyntaxKind.ObjectCreationExpression) && !IsList(semanticModel, (ObjectCreationExpressionSyntax)e))
						{
							return;
						}

						context.ReportDiagnostic(Rules.CreateDontConcatTwoCollectionsDefinedWithLiteralsDiagnostic(expression.GetLocation()));
					}
					else
					{
						switch (arguments.Count)
						{
							case 1:
								var e = expression.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)expression.Expression).GetExpression() : expression.Expression;

								if (e.IsKind(SyntaxKind.ObjectCreationExpression) && !IsList(semanticModel, (ObjectCreationExpressionSyntax)e))
								{
									return;
								}

								if (!IsIEnumerable(semanticModel, arguments[0].Expression))
								{
									return;
								}

								context.ReportDiagnostic(Rules.CreateDontUseConcatWhenPrependingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

								break;
							case 2:
								if (arguments[0].Expression.IsKind(SyntaxKind.ObjectCreationExpression))
								{
									if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[0].Expression))
									{
										return;
									}
								}

								if (!semanticModel.GetTypeInfo(expression.Expression).Type!.IsMatch(WellKnownTypeNames.Enumerable))
								{
									return;
								}

								context.ReportDiagnostic(Rules.CreateDontUseConcatWhenPrependingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

								break;
						}
					}
				}
				else if (LooksLikeAppend(invocation))
				{
					switch (arguments.Count)
					{
						case 1:

							if (arguments[0].Expression.IsKind(SyntaxKind.ObjectCreationExpression))
							{
								if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[0].Expression))
								{
									return;
								}
							}

							if (!IsIEnumerable(semanticModel, expression.Expression))
							{
								return;
							}

							context.ReportDiagnostic(Rules.CreateDontUseConcatWhenAppendingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

							break;
						case 2:

							if (arguments[1].Expression.IsKind(SyntaxKind.ObjectCreationExpression))
							{
								if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[1].Expression))
								{
									return;
								}
							}

							if (!semanticModel.GetTypeInfo(expression.Expression).Type!.IsMatch(WellKnownTypeNames.Enumerable))
							{
								return;
							}

							context.ReportDiagnostic(Rules.CreateDontUseConcatWhenAppendingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

							break;
					}
				}
			}
		}
	}
}
#pragma warning restore CA1303



