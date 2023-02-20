using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class LinqEnumerableAnalyzer : DiagnosticAnalyzer
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

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp3))
			{
				return;
			}

			/* exclusively checking the existence of the Enumerable.Append method because 
			   Enumerable.Append and Enumerable.Prepend were released at the same time */
			var hasEnumerableAppendMethod = HasEnumerableAppendMethod(context.Compilation);
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(c => Analyze(c, cache, hasEnumerableAppendMethod), SyntaxKind.SimpleMemberAccessExpression);
		}

		static bool HasEnumerableAppendMethod(Compilation compilation)
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

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache, bool hasEnumerableAppendMethod)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var expression = (MemberAccessExpressionSyntax)context.Node;

			if (expression == null)
			{
				return;
			}

			if (!string.Equals(expression.Name.Identifier.Text, nameof(Enumerable.Concat), StringComparison.Ordinal))
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

			if (invocation.ArgumentList.Arguments.Count is not (1 or 2))
			{
				return;
			}

			var semanticModel = context.SemanticModel;
			var arguments = invocation.ArgumentList.Arguments;

			if (LooksLikeShouldBePrepend(invocation))
			{
				if (LooksLikeShouldBeAppend(invocation))
				{
					var e = expression.Expression.TryGetExpressionFromParenthesizedExpression();

					foreach (var argument in arguments)
					{
						if (!IsSupportedCollection(semanticModel, argument.Expression))
						{
							return;
						}
					}

					if (arguments.Count == 1 && !IsSupportedCollection(semanticModel, e))
					{
						return;
					}

					context.ReportDiagnostic(Rules.CreateDontConcatTwoCollectionsDefinedWithLiteralsDiagnostic(expression.GetLocation()));
				}
				else
				{
					if (!hasEnumerableAppendMethod)
					{
						return;
					}

					var e = CheckConcatExpressionMeetsSemanticRequirements(semanticModel, invocation);

					if (e == null)
					{
						return;
					}

					context.ReportDiagnostic(Rules.CreateDontUseConcatWhenPrependingSingleElementToEnumerablesDiagnostic(e.GetLocation()));
				}
			}
			else if (LooksLikeShouldBeAppend(invocation))
			{
				if (!hasEnumerableAppendMethod)
				{
					return;
				}

				var e = CheckConcatExpressionMeetsSemanticRequirements(semanticModel, invocation);

				if (e == null)
				{
					return;
				}

				context.ReportDiagnostic(Rules.CreateDontUseConcatWhenAppendingSingleElementToEnumerablesDiagnostic(e.GetLocation()));
			}
		}

		static bool LooksLikeShouldBePrepend(InvocationExpressionSyntax invocation)
		{
			if (invocation.ArgumentList.Arguments.Count == 1)
			{
				var expression = ((MemberAccessExpressionSyntax)invocation.Expression).Expression.TryGetExpressionFromParenthesizedExpression();
				return ContainsSingleElement(expression);
			}

			return ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression);
		}

		static bool LooksLikeShouldBeAppend(InvocationExpressionSyntax invocation) => (invocation.ArgumentList.Arguments.Count == 1 ? ContainsSingleElement(invocation.ArgumentList.Arguments[0].Expression) : ContainsSingleElement(invocation.ArgumentList.Arguments[1].Expression));

		static bool ContainsSingleElement(ExpressionSyntax? e)
		{
			var maybeExpressions = LinqEnumerableUtils.GetInitializer(e)?.Expressions;
			if (!maybeExpressions.HasValue)
			{
				return false;
			}

			var expressions = maybeExpressions.GetValueOrDefault();
			if (expressions.Count != 1)
			{
				return false;
			}

			var expression = expressions[0];

			if (expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
			{
				return false;
			}

			return true;
		}

		static ExpressionSyntax? CheckConcatExpressionMeetsSemanticRequirements(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
		{
			var arguments = invocation.ArgumentList.Arguments;

			var e = ((MemberAccessExpressionSyntax)invocation.Expression).Expression.TryGetExpressionFromParenthesizedExpression();

			var concatMethodSymbol = (IMethodSymbol?)semanticModel.GetSymbolInfo(invocation.Expression).Symbol;

			if (concatMethodSymbol == null || !concatMethodSymbol.IsMatch(WellKnownTypeNames.Enumerable, nameof(Enumerable.Concat)))
			{
				return null;
			}

			switch (arguments.Count)
			{
				case 1:
					if (!(IsSupportedCollection(semanticModel, e) || IsSupportedCollection(semanticModel, arguments[0].Expression)))
					{
						return null;
					}

					break;

				case 2:
					var typeSymbol = semanticModel.GetTypeInfo(e).Type;

					if (typeSymbol == null || !typeSymbol.IsMatch(WellKnownTypeNames.Enumerable))
					{
						return null;
					}

					if (!AnyArgumentIsSupportedCollection(semanticModel, arguments))
					{
						return null;
					}
					
					break;

				default:
					return null;
			}

			return invocation.Expression;
		}

		static bool IsSupportedCollection(SemanticModel model, ExpressionSyntax e)
		{
			var typeSymbol = model.GetTypeInfo(e).Type;

			if (typeSymbol == null)
			{
				return false;
			}

			return typeSymbol.TypeKind == TypeKind.Array || typeSymbol.IsMatch(WellKnownTypeNames.List_T);
		}

		static bool AnyArgumentIsSupportedCollection (SemanticModel semanticModel, SeparatedSyntaxList<ArgumentSyntax> arguments)
		{
			foreach (var argument in arguments)
			{
				if (IsSupportedCollection(semanticModel, argument.Expression))
				{
					return true;
				}
			}

			return false;
		}
	}
}



