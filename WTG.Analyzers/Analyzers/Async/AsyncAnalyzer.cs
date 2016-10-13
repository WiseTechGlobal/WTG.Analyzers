using System;
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
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new[]
		{
			Rules.DoNotConfigureAwaitFromAsyncVoidRule,
		});

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context)
		{
			var invoke = (InvocationExpressionSyntax)context.Node;
			var member = invoke.Expression as MemberAccessExpressionSyntax;

			if (member != null &&
				member.Name.Identifier.Text == nameof(Task.ConfigureAwait) && // quick check before hitting the SemanticModel.
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
			return symbol.IsMatch("mscorlib", "System.Threading.Tasks.Task", nameof(Task.ConfigureAwait));
		}

		static IMethodSymbol GetInvokeMethod(INamedTypeSymbol delegateType)
		{
			foreach (var member in delegateType.GetMembers())
			{
				var method = member as IMethodSymbol;

				if (method != null && method.Name == nameof(Action.Invoke))
				{
					return method;
				}
			}

			return null;
		}
	}
}
