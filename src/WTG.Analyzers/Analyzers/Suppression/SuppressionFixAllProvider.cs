using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class SuppressionFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static SuppressionFixAllProvider Instance { get; } = new SuppressionFixAllProvider();

		SuppressionFixAllProvider()
		{
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await documentToFix.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var nodesToRemove = GetNodesToRemove(root, diagnostics);

			var newRoot = root.RemoveNodes(
				nodesToRemove,
				SyntaxRemoveOptions.KeepNoTrivia);

			NRT.Assert(newRoot != null, "Should only delete the suppressions, not the entire document.");

			return documentToFix.WithSyntaxRoot(newRoot);
		}

		static IEnumerable<SyntaxNode> GetNodesToRemove(SyntaxNode root, ImmutableArray<Diagnostic> diagnostics)
		{
			var attributes = new List<AttributeSyntax>();
			var attributeLists = new List<AttributeListSyntax>();

			for (var i = 0; i < diagnostics.Length; i++)
			{
				var node = root.FindNode(diagnostics[i].Location.SourceSpan);

				switch (node.Kind())
				{
					case SyntaxKind.Attribute:
						attributes.Add((AttributeSyntax)node);
						break;

					case SyntaxKind.AttributeList:
						attributeLists.Add((AttributeListSyntax)node);
						break;
				}
			}

			if (attributes.Count == 0)
			{
				return attributeLists;
			}

			// If all the attributes in a list are to be removed, then remove the list instead.
			var attGroups = attributes.ToLookup(x =>
				{
					var parent = (AttributeListSyntax?)x.Parent;
					NRT.Assert(parent != null, "The fixer should only be running on a full and complete document.");
					return parent;
				});
			attributes = new List<AttributeSyntax>();

			foreach (var group in attGroups)
			{
				if (group.Key.Attributes.Count == group.Count())
				{
					attributeLists.Add(group.Key);
				}
				else
				{
					attributes.AddRange(group);
				}
			}

			if (attributes.Count == 0)
			{
				return attributeLists;
			}
			else if (attributeLists.Count == 0)
			{
				return attributes;
			}
			else
			{
				return attributes.Concat<SyntaxNode>(attributeLists);
			}
		}
	}
}
