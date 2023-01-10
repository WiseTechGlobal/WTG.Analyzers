using System;
using System.Collections.Immutable;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Analyzers.BooleanLiteral;
using WTG.Analyzers.Analyzers.LinqEnumerable;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
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

			/* exclusively checking the existence of the Enumerable.Append method because 
			   Enumerable.Append and Enumerable.Prepend were released at the same time */
			var hasEnumerableAppendMethod = HasEnumerableAppendMethod(context.Compilation);
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(c => Analyze(c, cache, hasEnumerableAppendMethod), SyntaxKind.SimpleMemberAccessExpression);
		}

		static bool ContainsSingleElement(ExpressionSyntax? e) => LinqEnumerableUtils.GetInitializer(e)?.Expressions.Count == 1;

		public static bool LooksLikeAppend(InvocationExpressionSyntax invocation) => (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[1].Expression));

		public static bool LooksLikePrepend(InvocationExpressionSyntax invocation)
		{
			var expression = ((MemberAccessExpressionSyntax)invocation.Expression).Expression.TryGetExpressionFromParenthesizedExpression();

			if (expression == null)
			{
				expression = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
			}

			return (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression));
		}

		public static bool IsMonadicArityCollection (SemanticModel model, ExpressionSyntax e)
		{
			var typeSymbol = model.GetTypeInfo(e).Type;

			if (typeSymbol == null)
			{
				return false;
			}

			if (typeSymbol.MetadataName.Length != 0 && typeSymbol.MetadataName[typeSymbol.MetadataName.Length - 1] != '1')
			{
				return false;
			}

			foreach (var baseType in typeSymbol.AllInterfaces)
			{
				if (baseType.IsMatch(WellKnownTypeNames.IEnumerable_T))
				{
					return true;
				}
			}

			return typeSymbol.IsMatch(WellKnownTypeNames.IEnumerable_T);
		}

		public static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache, bool hasEnumerableAppendMethod)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken) || !hasEnumerableAppendMethod)
			{
				return;
			}

			var expression = (MemberAccessExpressionSyntax)context.Node;

			if (expression == null)
			{
				return;
			}

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

			if (!(invocation.ArgumentList.Arguments.Count is 1 or 2))
			{
				return;
			}

			var semanticModel = context.SemanticModel;
			var arguments = invocation.ArgumentList.Arguments;

			if (LooksLikePrepend(invocation))
			{
				if (LooksLikeAppend(invocation))
				{
					var e = expression.Expression.TryGetExpressionFromParenthesizedExpression();

					if (e == null)
					{
						e = expression.Expression;
					}

					foreach (var argument in arguments)
					{
						if (!IsMonadicArityCollection(semanticModel, argument.Expression))
						{
							return;
						}
					}

					if (arguments.Count == 1 && !IsMonadicArityCollection(semanticModel, e))
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
							var e = expression.Expression.TryGetExpressionFromParenthesizedExpression();

							if (e == null)
							{
								e = expression.Expression;
							}

							if (!IsMonadicArityCollection(semanticModel, e) || !IsMonadicArityCollection(semanticModel, arguments[0].Expression))
							{
								return;
							}

							context.ReportDiagnostic(Rules.CreateDontUseConcatWhenPrependingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

							break;

						case 2:
							var typeSymbol = semanticModel.GetTypeInfo(expression.Expression).Type;

							/* An instance of String.Concat() is more likely than arguments being non-monadic 
							   arity collections, so this check is done first */
							if (typeSymbol == null || !typeSymbol.IsMatch(WellKnownTypeNames.Enumerable))
							{
								return;
							}

							foreach (var argument in arguments)
							{
								if (!IsMonadicArityCollection(semanticModel, argument.Expression))
								{
									return;
								}
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

						if (!IsMonadicArityCollection(semanticModel, expression.Expression) ||
							!IsMonadicArityCollection(semanticModel, arguments[0].Expression))
						{
							return;
						}

						context.ReportDiagnostic(Rules.CreateDontUseConcatWhenAppendingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

						break;
					case 2:

						var typeSymbol = semanticModel.GetTypeInfo(expression.Expression).Type;

						if (typeSymbol == null || !typeSymbol.IsMatch(WellKnownTypeNames.Enumerable))
						{
							return;
						}

						foreach (var argument in arguments)
						{
							if (!IsMonadicArityCollection(semanticModel, argument.Expression))
							{
								return;
							}
						}

						context.ReportDiagnostic(Rules.CreateDontUseConcatWhenAppendingSingleElementToEnumerablesDiagnostic(expression.GetLocation()));

						break;
				}
			}
		}
	}
}



