using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class TypeComparisonFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static TypeComparisonFixAllProvider Instance { get; } = new TypeComparisonFixAllProvider();

		TypeComparisonFixAllProvider()
		{
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await documentToFix.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (diagnostics.Length == 1)
			{
				var diagnosticSpan = diagnostics[0].Location.SourceSpan;
				var node = (BinaryExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
				return documentToFix.WithSyntaxRoot(ExpressionRemover.ReplaceWithConstantBool(root, node, !node.IsKind(SyntaxKind.EqualsExpression)));
			}
			else
			{
				var builder = ImmutableDictionary.CreateBuilder<SyntaxNode, bool>();

				foreach (var diagnostic in diagnostics)
				{
					var diagnosticSpan = diagnostic.Location.SourceSpan;
					var node = (BinaryExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
					builder.Add(node, !node.IsKind(SyntaxKind.EqualsExpression));
				}

				return documentToFix.WithSyntaxRoot(ExpressionRemover.ReplaceWithConstantBool(root, builder.ToImmutable()));
			}
		}
	}
}
