using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitCompletedCodeFixProvider))]
	[Shared]
	public sealed class AwaitCompletedCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontAwaitTriviallyCompletedTasksDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove ConfigureAwait()",
					createChangedDocument: c => RemoveAwaitAsync(context.Document, diagnostic, c),
					equivalenceKey: "RemoveConfigureAwait"),
				diagnostic: diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> RemoveAwaitAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (AwaitExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			if (diagnostic.AdditionalLocations.Count == 1)
			{
				var replacementNode = node.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true);

				return document.WithSyntaxRoot(
					root.ReplaceNode(
						node,
						replacementNode));
			}

			return document.WithSyntaxRoot(
				root.RemoveNode(
					node.Parent.IsKind(SyntaxKind.ExpressionStatement)
						? node.Parent
						: node,
					SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.AddElasticMarker));
		}
	}
}
