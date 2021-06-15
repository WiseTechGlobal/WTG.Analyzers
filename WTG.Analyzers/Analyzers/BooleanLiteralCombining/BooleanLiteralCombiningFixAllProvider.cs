using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class BooleanLiteralCombiningFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static BooleanLiteralCombiningFixAllProvider Instance { get; } = new BooleanLiteralCombiningFixAllProvider();

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var replacements = ImmutableDictionary.CreateBuilder<SyntaxNode, bool>();

			foreach (var diagnostic in diagnostics)
			{
				var expression = (LiteralExpressionSyntax)root.FindNode(
					diagnostic.Location.SourceSpan,
					getInnermostNodeForTie: true);

				replacements.Add(expression, expression.IsKind(SyntaxKind.TrueLiteralExpression));
			}

			return documentToFix.WithSyntaxRoot(
				ExpressionRemover.ReplaceWithConstantBool(root, replacements.ToImmutable()));
		}
	}
}
