using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncCodeFixProvider))]
	[Shared]
	public sealed class AsyncCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotConfigureAwaitFromAsyncVoidDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove ConfigureAwait()",
					createChangedDocument: c => RemoveConfigureAwait(context.Document, diagnostic, c),
					equivalenceKey: "RemoveConfigureAwait"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveConfigureAwait(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);

			var invoke = (InvocationExpressionSyntax)node;
			var method = (MemberAccessExpressionSyntax)invoke.Expression;

			if (method.Expression != null)
			{
				var newNode = method.Expression
					.WithLeadingTrivia(node.GetLeadingTrivia())
					.WithTrailingTrivia(node.GetTrailingTrivia());

				var newRoot = root.ReplaceNode(node, newNode);
				document = document.WithSyntaxRoot(newRoot);
			}

			return document;
		}
	}
}
