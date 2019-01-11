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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeContractsCodeFixProvider))]
	[Shared]
	public sealed class CodeContractsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseCodeContractsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			if (diagnostic.Properties.TryGetValue(CodeContractsAnalyzer.PropertyProposedFix, out var proposedFix))
			{
				switch (proposedFix)
				{
					case CodeContractsAnalyzer.FixDelete:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Remove Contract Usage.",
								c => FixByDelete(context.Document, diagnostic, c),
								equivalenceKey: "FIX"),
							diagnostic);
						break;
				}
			}

			return Task.FromResult<object>(null);
		}

		static async Task<Document> FixByDelete(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.RemoveNode(node, SyntaxRemoveOptions.AddElasticMarker | SyntaxRemoveOptions.KeepExteriorTrivia));
		}
	}
}
