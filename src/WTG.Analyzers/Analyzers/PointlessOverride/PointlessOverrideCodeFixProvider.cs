using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PointlessOverrideCodeFixProvider))]
	[Shared]
	public sealed class PointlessOverrideCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.RemovePointlessOverridesDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => PointlessOverrideFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove overriding member.",
					createChangedDocument: c => RemoveOverideAsync(context.Document, diagnostic, c),
					equivalenceKey: "RemoveOverride"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveOverideAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.AddElasticMarker);
			NRT.Assert(newRoot != null, "Should only delete the override, not the entire document.");

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
