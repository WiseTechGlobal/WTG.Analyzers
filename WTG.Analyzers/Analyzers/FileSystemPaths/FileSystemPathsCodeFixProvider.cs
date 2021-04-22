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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileSystemPathsCodeFixProvider))]
	[Shared]
	public sealed class FileSystemPathsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUsePathSeparatorsInPathLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Use Path.Combine(...)",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "UsePathCombine"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			_ = diagnostic;
			_ = cancellationToken;
			await Task.Yield();

			return document;
		}
	}
}
