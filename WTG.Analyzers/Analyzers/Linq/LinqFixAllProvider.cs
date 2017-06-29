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

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
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
