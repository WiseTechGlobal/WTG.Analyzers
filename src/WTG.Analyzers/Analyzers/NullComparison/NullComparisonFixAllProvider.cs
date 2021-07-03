using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class NullComparisonFixAllProvider : DocumentBatchedFixAllProvider
	{
		NullComparisonFixAllProvider()
		{
		}
		public static NullComparisonFixAllProvider Instance { get; } = new NullComparisonFixAllProvider();

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var lookup = ImmutableDictionary.CreateBuilder<SyntaxNode, bool>();

			foreach (var diagnostic in diagnostics)
			{
				var diagnosticSpan = diagnostic.Location.SourceSpan;
				var node = (BinaryExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

				switch (node.Kind())
				{
					case SyntaxKind.EqualsExpression:
						lookup.Add(node, false);
						break;

					case SyntaxKind.NotEqualsExpression:
						lookup.Add(node, true);
						break;

					default:
						throw new InvalidOperationException("Unreachable - encountered unexpected syntax node " + node.Kind());
				}
			}

			return documentToFix.WithSyntaxRoot(ExpressionRemover.ReplaceWithConstantBool(root, lookup.ToImmutable()));
		}
	}
}
