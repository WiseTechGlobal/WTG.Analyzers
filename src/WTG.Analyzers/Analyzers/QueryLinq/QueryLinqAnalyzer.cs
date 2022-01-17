using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class QueryLinqAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UsingEnumerableExtensionMethodsOnAQueryableRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();
			context.RegisterSyntaxNodeAction(c => Analyze(c, cache), SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)context.Node;

			if (!invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				return;
			}

			var memberAccessExpression = (MemberAccessExpressionSyntax)invoke.Expression;

			if (memberAccessExpression.Expression != null &&
				TargetExtensionMethods.Contains(memberAccessExpression.Name.Identifier.Text) &&
				IsQueryable(context.SemanticModel, memberAccessExpression.Expression, context.CancellationToken) &&
				IsEnumerableExtensionMethod(context.SemanticModel, invoke, context.CancellationToken))
			{
				var location = Location.Create(
					invoke.SyntaxTree,
					TextSpan.FromBounds(
						memberAccessExpression.Name.SpanStart,
						invoke.ArgumentList.Span.End));

				context.ReportDiagnostic(
					Rules.CreateUsingEnumerableExtensionMethodsOnAQueryableDiagnostic(location));
			}
		}

		static bool IsEnumerableExtensionMethod(SemanticModel model, InvocationExpressionSyntax invoke, CancellationToken cancellationToken)
		{
			var methodSymbol = (IMethodSymbol?)model.GetSymbolInfo(invoke, cancellationToken).Symbol;

			if (methodSymbol == null || !methodSymbol.IsExtensionMethod)
			{
				return false;
			}

			var containingSymbol = methodSymbol.OriginalDefinition.ContainingType;

			if (containingSymbol.Kind != SymbolKind.NamedType)
			{
				return false;
			}

			return ((ITypeSymbol)containingSymbol).IsMatch(WellKnownTypeNames.Enumerable);
		}

		static bool IsQueryable(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var typeSymbol = model.GetTypeInfo(expression, cancellationToken).Type;

			if (typeSymbol == null || typeSymbol.Kind != SymbolKind.NamedType)
			{
				return false;
			}

			var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;

			if (IsQueryable(namedTypeSymbol))
			{
				return true;
			}

			foreach (var iface in namedTypeSymbol.AllInterfaces)
			{
				if (IsQueryable(iface))
				{
					return true;
				}
			}

			return false;

			static bool IsQueryable(INamedTypeSymbol type) => type.IsGenericType && type.IsMatch(WellKnownTypeNames.IQueryable_T);
		}

		static readonly ImmutableHashSet<string> TargetExtensionMethods = ImmutableHashSet.Create(
			nameof(Enumerable.Where),
			nameof(Enumerable.Select),
			nameof(Enumerable.OrderBy),
			nameof(Enumerable.Count),
			nameof(Enumerable.All),
			nameof(Enumerable.Any),
			nameof(Enumerable.First),
			nameof(Enumerable.Single),
			nameof(Enumerable.FirstOrDefault),
			nameof(Enumerable.SingleOrDefault));
	}
}
