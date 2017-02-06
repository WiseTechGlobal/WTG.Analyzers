using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VisibilityCodeFixProvider)), Shared]
	public sealed class VisibilityCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseThePrivateKeywordDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove private",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "RemovePrivate"),
				diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
			var root = editor.OriginalRoot;

			var token = TokenFromDiagnostic(root, diagnostic);
			var nextToken = token.GetNextToken();

			var newRoot = root.ReplaceTokens(
				new[] { token, nextToken },
				(original, current) =>
				{
					if (original.IsKind(SyntaxKind.PrivateKeyword))
					{
						return SyntaxFactory.Token(SyntaxKind.None);
					}
					else
					{
						return current.WithLeadingTrivia(token.LeadingTrivia);
					}
				});

			return document.WithSyntaxRoot(newRoot);
		}

		static SyntaxToken TokenFromDiagnostic(SyntaxNode root, Diagnostic diagnostic)
		{
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			return root.FindToken(diagnosticSpan.Start);
		}
	}
}
