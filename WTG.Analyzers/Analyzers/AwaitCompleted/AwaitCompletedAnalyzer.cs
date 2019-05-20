using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class AwaitCompletedAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontAwaitTriviallyCompletedTasksRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.AwaitExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (AwaitExpressionSyntax)context.Node;

			if (IsAwaitTargetTrivial(context.SemanticModel, node, out var innerValueLocation, context.CancellationToken))
			{
				var additionalLocations = innerValueLocation == null
					? Enumerable.Empty<Location>()
					: new[] { innerValueLocation };

				context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.DontAwaitTriviallyCompletedTasksRule,
					node.GetLocation(),
					additionalLocations));
			}
		}

		static bool IsAwaitTargetTrivial(SemanticModel model, AwaitExpressionSyntax expression, out Location innerValueLocation, CancellationToken cancellationToken)
		{
			switch (expression.Expression.Kind())
			{
				case SyntaxKind.SimpleMemberAccessExpression:
					innerValueLocation = null;
					return IsCompletedTask(model, (MemberAccessExpressionSyntax)expression.Expression, cancellationToken);

				case SyntaxKind.InvocationExpression:
					return IsFromResult(model, (InvocationExpressionSyntax)expression.Expression, out innerValueLocation, cancellationToken);
			}

			innerValueLocation = null;
			return false;
		}

		static bool IsCompletedTask(SemanticModel model, MemberAccessExpressionSyntax expression, CancellationToken cancellationToken)
		{
			if (expression.Name.Identifier.Text != nameof(Task.CompletedTask))
			{
				return false;
			}

			var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;

			if (symbol == null || symbol.Kind != SymbolKind.Property)
			{
				return false;
			}

			var property = (IPropertySymbol)symbol;

			return property.IsMatch("System.Threading.Tasks.Task", nameof(Task.CompletedTask));
		}

		static bool IsFromResult(SemanticModel model, InvocationExpressionSyntax expression, out Location innerValueLocation, CancellationToken cancellationToken)
		{
			if (!expression.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				innerValueLocation = null;
				return false;
			}

			var member = (MemberAccessExpressionSyntax)expression.Expression;

			if (member.Name.Identifier.Text != nameof(Task.FromResult))
			{
				innerValueLocation = null;
				return false;
			}

			var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;

			if (symbol == null || symbol.Kind != SymbolKind.Method)
			{
				innerValueLocation = null;
				return false;
			}

			innerValueLocation = expression.ArgumentList.Arguments[0].Expression.GetLocation();

			return ((IMethodSymbol)symbol).IsMatch("System.Threading.Tasks.Task", nameof(Task.FromResult));
		}
	}
}
