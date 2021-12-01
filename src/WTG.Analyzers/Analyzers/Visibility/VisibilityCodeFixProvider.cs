using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VisibilityCodeFixProvider))]
	[Shared]
	public sealed class VisibilityCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseThePrivateKeywordDiagnosticID,
			Rules.DoNotUseTheInternalKeywordForTopLevelTypesDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.DoNotUseThePrivateKeywordDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Remove 'private' visibility modifier",
							createChangedDocument: c => RemoveKeyword(context.Document, diagnostic, c),
							equivalenceKey: "RemovePrivate"),
						diagnostic);
					break;

				case Rules.DoNotUseTheInternalKeywordForTopLevelTypesDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Remove 'internal' visibility modifier",
							createChangedDocument: c => RemoveKeyword(context.Document, diagnostic, c),
							equivalenceKey: "RemoveInternal"),
						diagnostic);
					break;
			}

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveKeyword(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var token = TokenFromDiagnostic(root, diagnostic);
			var nextToken = token.GetNextToken();

			var newRoot = root.ReplaceTokens(
				new[] { token, nextToken },
				(original, current) =>
				{
					var kind = original.Kind();

					if (kind == SyntaxKind.PrivateKeyword || kind == SyntaxKind.InternalKeyword)
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
