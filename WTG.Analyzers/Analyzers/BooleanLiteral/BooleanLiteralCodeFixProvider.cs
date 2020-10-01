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
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BooleanLiteralCodeFixProvider))]
	[Shared]
	public sealed class BooleanLiteralCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UseNamedArgumentsWhenPassingBooleanLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Use Named Arguments",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "UseNamedArguments"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			_ = diagnostic;
			_ = cancellationToken;
			return Task.FromResult(document);
		}
	}
}
