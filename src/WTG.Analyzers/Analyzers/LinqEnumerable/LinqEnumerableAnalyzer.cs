using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Xml.Schema;
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

		static bool ContainsSingleElement(ArrayCreationExpressionSyntax e) => (e != null && e.Initializer != null && e.Initializer.Expressions.Count == 1);
		static bool ContainsSingleElement(ImplicitArrayCreationExpressionSyntax e) => (e != null && e.Initializer != null && e.Initializer.Expressions.Count == 1);
		static bool ContainsSingleElement(ObjectCreationExpressionSyntax e) => (e != null && e.Initializer != null && e.Initializer.Expressions.Count == 1);

		static bool ContainsSingleElement (ExpressionSyntax e)
		{
			switch (e.Kind())
			{
				case SyntaxKind.ImplicitArrayCreationExpression:
					return ContainsSingleElement((ImplicitArrayCreationExpressionSyntax)e);
				case SyntaxKind.ObjectCreationExpression:
					return ContainsSingleElement((ObjectCreationExpressionSyntax)e);
				case SyntaxKind.ArrayCreationExpression:
					return ContainsSingleElement((ArrayCreationExpressionSyntax)e);
				default:
					return false;
			}
		}

		public static bool LooksLikeAppend(InvocationExpressionSyntax invocation) => (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[1].Expression));

		public static bool LooksLikePrepend(InvocationExpressionSyntax invocation)
		{
			var expression = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
			if (expression.Kind() == SyntaxKind.ParenthesizedExpression)
			{
				expression = ((ParenthesizedExpressionSyntax)expression).Expression;
			}

			return (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression));
		}

		public static bool IsList(SemanticModel model, ObjectCreationExpressionSyntax e) => (model.GetTypeInfo(e).Type!.MetadataName == "List`1");
		public static bool IsIEnumerable(SemanticModel model, ExpressionSyntax e) => (model.GetTypeInfo(e).Type!.MetadataName == "IEnumerable`1");

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

				var invocation = expression.Parent as InvocationExpressionSyntax;

				if (invocation == null)
				{
					return;
				}

				if (invocation.ArgumentList.Arguments.Count > 2)
				{
					return;
				}

				// caching OP and so forth
				var semanticModel = context.SemanticModel;
				// this is O(n) but, the O(1) alternative looks gross so for readability purposes I'm doing
				// unneccessary conversions
				var arguments = invocation.ArgumentList.Arguments.ToList().ConvertAll(x => x.Expression);

				if (LooksLikePrepend(invocation))
				{
					if (LooksLikeAppend(invocation))
					{
						foreach (var argument in arguments)
						{
							if (argument.IsKind(SyntaxKind.ObjectCreationExpression))
							{
								if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)argument))
								{
									return;
								}
							}
						}

						var e = expression.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)expression.Expression).Expression : expression.Expression;

						if (e.IsKind(SyntaxKind.ObjectCreationExpression))
						{
							if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)e))
							{
								return;
							}
						}

						context.ReportDiagnostic(Rules.CreateDontConcatTwoCollectionsDefinedWithLiteralsDiagnostic(expression.GetLocation()));
					}
					else
					{
						switch (arguments.Count)
						{
							case 1:
								var e = expression.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)expression.Expression).Expression : expression.Expression;

								if (e.IsKind(SyntaxKind.ObjectCreationExpression))
								{
									if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)e))
									{
										return;
									}
								}

								if (!IsIEnumerable(semanticModel, arguments[0]))
								{
									return;
								}

								context.ReportDiagnostic(Rules.CreateDontUseConcatWhenPrependingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

								break;
							case 2:
								if (arguments[0].IsKind(SyntaxKind.ObjectCreationExpression))
								{
									if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[0]))
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

							if (arguments[0].IsKind(SyntaxKind.ObjectCreationExpression))
							{
								if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[0]))
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

							if (arguments[1].IsKind(SyntaxKind.ObjectCreationExpression))
							{
								if (!IsList(semanticModel, (ObjectCreationExpressionSyntax)arguments[1]))
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
	#pragma warning restore CA1303
}



