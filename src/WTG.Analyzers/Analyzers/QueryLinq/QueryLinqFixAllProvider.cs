using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers
{
	sealed class QueryLinqFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static QueryLinqFixAllProvider Instance { get; } = new QueryLinqFixAllProvider();

		QueryLinqFixAllProvider()
		{
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var nodes = new SyntaxNode[diagnostics.Length];

			for (var i = 0; i < diagnostics.Length; i++)
			{
				nodes[i] = root.FindNode(diagnostics[i].Location.SourceSpan, getInnermostNodeForTie: true);
				root = root.ReplaceNode(nodes[i], AddAsEnumerable(nodes[i]));
			}

			return documentToFix.WithSyntaxRoot(root);
		}

		static SyntaxNode AddAsEnumerable(SyntaxNode node)
		{
			var invocationExpressionNode = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
			if (invocationExpressionNode.Expression is not MemberAccessExpressionSyntax memberAccessExpressionNode)
			{
				return node;
			}

			var newNode = invocationExpressionNode.WithExpression(MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							InvocationExpression(
								MemberAccessExpression(
									SyntaxKind.SimpleMemberAccessExpression,
									memberAccessExpressionNode.Expression,
									IdentifierName("AsEnumerable"))),
							memberAccessExpressionNode.Name))
					.WithArgumentList(invocationExpressionNode.ArgumentList)
					.NormalizeWhitespace();

			return newNode;
		}
	}
}
