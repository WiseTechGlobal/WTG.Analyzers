using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class AsyncAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotConfigureAwaitFromAsyncVoidRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context)
		{
			var invoke = (InvocationExpressionSyntax)context.Node;

			if (invoke.Expression is MemberAccessExpressionSyntax member &&
				member.Name.Identifier.Text == nameof(Task.ConfigureAwait) && // quick check before hitting the SemanticModel.
				!HasLiteralTrueArgument(invoke) &&
				IsConfigureAwait(context.SemanticModel, invoke) &&
				IsWithinAsyncVoidMethod(context.SemanticModel, invoke))
			{
				context.ReportDiagnostic(CreateDiagnostic(invoke, member));
			}
		}

		static Diagnostic CreateDiagnostic(InvocationExpressionSyntax invoke, MemberAccessExpressionSyntax member)
		{
			var span = TextSpan.FromBounds(
				member.OperatorToken.GetLocation().SourceSpan.Start,
				invoke.ArgumentList.CloseParenToken.GetLocation().SourceSpan.End);

			return Rules.CreateDoNotConfigureAwaitFromAsyncVoidDiagnostic(Location.Create(invoke.SyntaxTree, span));
		}

		static bool IsWithinAsyncVoidMethod(SemanticModel model, SyntaxNode node)
		{
			var visitor = new IsAsyncVoidMethodVisitor(model);

			while (node != null)
			{
				var tmp = visitor.Visit(node);

				if (tmp.HasValue)
				{
					return tmp.Value;
				}

				node = node.Parent;
			}

			return false;
		}

		static bool IsConfigureAwait(SemanticModel semanticModel, InvocationExpressionSyntax invoke)
		{
			var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invoke).Symbol;
			return symbol != null && symbol.IsMatch("System.Threading.Tasks.Task", nameof(Task.ConfigureAwait));
		}

		static bool HasLiteralTrueArgument(InvocationExpressionSyntax invoke)
		{
			var arguments = invoke.ArgumentList.Arguments;

			return arguments.Count == 1
				&& arguments[0].Expression.IsKind(SyntaxKind.TrueLiteralExpression);
		}
	}
}
