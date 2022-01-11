using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class PointlessOverrideFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static PointlessOverrideFixAllProvider Instance { get; } = new PointlessOverrideFixAllProvider();

		PointlessOverrideFixAllProvider()
		{
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var nodes = new SyntaxNode[diagnostics.Length];

			for (var i = 0; i < diagnostics.Length; i++)
			{
				nodes[i] = root.FindNode(diagnostics[i].Location.SourceSpan, getInnermostNodeForTie: true);
			}

			var newRoot = root.RemoveNodes(
				nodes,
				SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.AddElasticMarker);

			NRT.Assert(newRoot != null, "Should only delete the overrides, not the entire document.");

			return documentToFix.WithSyntaxRoot(newRoot);
		}
	}
}
