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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuppressionCodeFixProvider))]
	[Shared]
	public sealed class SuppressionCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.RemovedOrphanedSuppressionsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => SuppressionFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove suppression",
					createChangedDocument: c => RemoveSuppresson(context.Document, diagnostic, c),
					equivalenceKey: "RemoveSuppression"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveSuppresson(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
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

			return root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
		}
	}
}
