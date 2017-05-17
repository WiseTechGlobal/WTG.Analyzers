using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class LinqFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static LinqFixAllProvider Instance { get; } = new LinqFixAllProvider();

		LinqFixAllProvider()
		{
		}

		protected override Task<Document> ApplyFixesAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			return ApplyFixesAsync(document, document, diagnostics, cancellationToken);
		}

		protected override async Task<Solution> ApplyFixesAsync(Solution solution, ImmutableDictionary<Document, ImmutableArray<Diagnostic>> diagnostics, CancellationToken cancellationToken)
		{
			foreach (var pair in diagnostics)
			{
				var originalDocument = pair.Key;
				var documentToFix = solution.GetDocument(originalDocument.Id);
				var semanticModel = await originalDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
				var newDocument = await ApplyFixesAsync(originalDocument, documentToFix, pair.Value, cancellationToken).ConfigureAwait(false);
				solution = newDocument.Project.Solution;
			}

			return solution;
		}

		static async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			// Semantic information should only be gotten from originalDocument (to avoid recompilation),
			// but we should make our changes to documentToFix (so that we don't discard fixes made to other documents in the project).
			// If we only ever make changes to documentToFix, then the syntax trees for the two documents will be the same.
			var root = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await originalDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var nodes = new InvocationExpressionSyntax[diagnostics.Length];

			for (var i = 0; i < diagnostics.Length; i++)
			{
				nodes[i] = (InvocationExpressionSyntax)root.FindNode(diagnostics[i].Location.SourceSpan, getInnermostNodeForTie: true);
			}

			return documentToFix.WithSyntaxRoot(
				root.ReplaceNodes(
					nodes,
					(InvocationExpressionSyntax original, InvocationExpressionSyntax modified) =>
					{
						var resolution = LinqUtils.GetResolution(semanticModel, original);
						return resolution == null ? modified : resolution.ApplyFix(modified);
					}));
		}
	}
}
