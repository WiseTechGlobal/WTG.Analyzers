using System.Collections.Generic;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuppressionCodeFixProvider)), Shared]
	public sealed class SuppressionCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Rules.RemovedOrphanedSuppressionsDiagnosticID); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove suppression",
					createChangedDocument: c => RemoveSuppresson(context.Document, diagnostic, c),
					equivalenceKey: "RemoveSuppression"),
				diagnostic);

			return Task.FromResult<object>(null);
		}

		async static Task<Document> RemoveSuppresson(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);

			return document.WithSyntaxRoot(RemoveNode(root, node));
		}

		static SyntaxNode RemoveNode(SyntaxNode root, SyntaxNode node)
		{
			if (node.IsKind(SyntaxKind.Attribute))
			{
				var attributeList = (AttributeListSyntax)node.Parent;

				if (attributeList.Attributes.Count == 1)
				{
					node = attributeList;
				}
			}

			return RemoveNodeWithSaneTriviaHandling(root, node);
		}

		static SyntaxNode RemoveNodeWithSaneTriviaHandling(SyntaxNode root, SyntaxNode nodeToRemove)
		{
			var leading = nodeToRemove.GetLeadingTrivia();
			var lastLeadingNewLineIndex = -1;
			var lastLeadingNonWhitespaceIndex = -1;

			for (var i = 0; i < leading.Count; i++)
			{
				var kind = leading[i].Kind();

				switch (kind)
				{
					case SyntaxKind.WhitespaceTrivia:
						break;

					case SyntaxKind.EndOfLineTrivia:
						lastLeadingNewLineIndex = i;
						goto default;

					default:
						lastLeadingNonWhitespaceIndex = i;
						break;
				}
			}

			var trailing = nodeToRemove.GetTrailingTrivia();
			var firstTrailingNewLineIndex = -1;
			var firstTrailingNonWhitespaceIndex = -1;

			for (var i = trailing.Count - 1; i >= 0; i--)
			{
				var kind = trailing[i].Kind();

				switch (kind)
				{
					case SyntaxKind.WhitespaceTrivia:
						break;

					case SyntaxKind.EndOfLineTrivia:
						firstTrailingNewLineIndex = i;
						goto default;

					default:
						firstTrailingNonWhitespaceIndex = i;
						break;
				}
			}

			return root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
		}

		static IEnumerable<T> ConcatSafe<T>(IEnumerable<T> x, IEnumerable<T> y)
		{
			if (x == null)
			{
				return y;
			}

			if (y == null)
			{
				return x;
			}

			return x.Concat(y);
		}
	}
}
