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
		public const string SourcePropertyName = "source";

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

			if (IsAwaitTargetTrivial(context.SemanticModel, node, out var properties, out var innerValueLocation, context.CancellationToken))
			{
				var additionalLocations = innerValueLocation == null
					? Enumerable.Empty<Location>()
					: new[] { innerValueLocation };

				context.ReportDiagnostic(
					Diagnostic.Create(
						Rules.DontAwaitTriviallyCompletedTasksRule,
						node.GetLocation(),
						additionalLocations,
						properties));
			}
		}

		static bool IsAwaitTargetTrivial(SemanticModel model, AwaitExpressionSyntax expression, out ImmutableDictionary<string, string> properties, out Location innerValueLocation, CancellationToken cancellationToken)
		{
			if (!TryUnwrapConfigureAwait(model, expression.Expression, out var taskExpression, cancellationToken))
			{
				taskExpression = expression.Expression;
			}

			switch (taskExpression.Kind())
			{
				case SyntaxKind.SimpleMemberAccessExpression:
					innerValueLocation = null;
					properties = CompletedTaskProperties;
					return IsCompletedTaskProperty(model, (MemberAccessExpressionSyntax)taskExpression, cancellationToken);

				case SyntaxKind.InvocationExpression:
					var invoke = (InvocationExpressionSyntax)taskExpression;

					if (IsCompletedTaskFactoryMethod(model, invoke, out properties, cancellationToken))
					{
						innerValueLocation = invoke.ArgumentList.Arguments[0].Expression.GetLocation();
						return true;
					}

					break;
			}

			innerValueLocation = null;
			properties = null;
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

		static bool IsCompletedTaskProperty(SemanticModel model, MemberAccessExpressionSyntax expression, CancellationToken cancellationToken)
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

		static bool IsCompletedTaskFactoryMethod(SemanticModel model, InvocationExpressionSyntax expression, out ImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
		{
			if (!expression.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				properties = null;
				return false;
			}

			var memberExpression = (MemberAccessExpressionSyntax)expression.Expression;

			switch (memberExpression.Name.Identifier.Text)
			{
				case nameof(Task.FromResult):
					properties = FromResultProperties;
					break;

				case nameof(Task.FromException):
					properties = FromExceptionProperties;
					break;

				case nameof(Task.FromCanceled):
					properties = ImmutableDictionary<string, string>.Empty;
					break;

				default:
					properties = null;
					return false;
			}

			var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;

			return symbol != null
				&& symbol.Kind == SymbolKind.Method
				&& symbol.ContainingType.IsMatch(WellKnownTypeNames.Task);
		}

		static readonly ImmutableDictionary<string, string> CompletedTaskProperties = ImmutableDictionary<string, string>.Empty.Add(SourcePropertyName, nameof(Task.CompletedTask));
		static readonly ImmutableDictionary<string, string> FromResultProperties = ImmutableDictionary<string, string>.Empty.Add(SourcePropertyName, nameof(Task.FromResult));
		static readonly ImmutableDictionary<string, string> FromExceptionProperties = ImmutableDictionary<string, string>.Empty.Add(SourcePropertyName, nameof(Task.FromException));
	}
}
