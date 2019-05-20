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
			if (!TryUnwrapConfigureAwait(model, expression.Expression, out var taskExpression, cancellationToken))
			{
				taskExpression = expression.Expression;
			}

			switch (taskExpression.Kind())
			{
				case SyntaxKind.SimpleMemberAccessExpression:
					innerValueLocation = null;
					return IsCompletedTask(model, (MemberAccessExpressionSyntax)taskExpression, cancellationToken);

				case SyntaxKind.InvocationExpression:
					return IsFromResult(model, (InvocationExpressionSyntax)taskExpression, out innerValueLocation, cancellationToken);
			}

			innerValueLocation = null;
			return false;
		}

		static bool TryUnwrapConfigureAwait(SemanticModel semanticModel, ExpressionSyntax node, out ExpressionSyntax taskExpression, CancellationToken cancellationToken)
		{
			if (node.IsKind(SyntaxKind.InvocationExpression))
			{
				var invoke = (InvocationExpressionSyntax)node;

				if (invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
				{
					var member = (MemberAccessExpressionSyntax)invoke.Expression;

					if (member.Name.Identifier.Text == nameof(Task.ConfigureAwait) && IsConfigureAwait(invoke))
					{
						taskExpression = member.Expression;
						return true;
					}
				}
			}

			taskExpression = node;
			return false;

			bool IsConfigureAwait(InvocationExpressionSyntax invoke)
			{
				var symbol = semanticModel.GetSymbolInfo(invoke, cancellationToken).Symbol;

				if (symbol == null || symbol.Kind != SymbolKind.Method)
				{
					return false;
				}

				var method = (IMethodSymbol)symbol;

				return method.Name == nameof(Task.ConfigureAwait)
					&& method.ContainingType.IsMatchAnyArity(WellKnownTypeNames.Task);
			}
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

			return property.IsMatch(WellKnownTypeNames.Task, nameof(Task.CompletedTask));
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

			return ((IMethodSymbol)symbol).IsMatch(WellKnownTypeNames.Task, nameof(Task.FromResult));
		}
	}
}
