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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WhitespaceCodeFixProvider)), Shared]
	public sealed class WhitespaceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Rules.DoNotLeaveWhitespaceOnTheEndOfTheLineDiagnosticID); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Remove trailing whitespace.",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "RemoveTrailingWhitespace"),
				diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var trivia = root.FindTrivia(diagnosticSpan.Start);
			var token = trivia.Token;

			return document.WithSyntaxRoot(
				root.ReplaceToken(
					token,
					RemoveTrivia(token, trivia)));
		}

		static SyntaxToken RemoveTrivia(SyntaxToken token, SyntaxTrivia trivia)
		{
			var index = token.TrailingTrivia.IndexOf(trivia);

			if (index >= 0)
			{
				return token.WithTrailingTrivia(token.TrailingTrivia.RemoveAt(index));
			}

			index = token.LeadingTrivia.IndexOf(trivia);

			if (index >= 0)
			{
				return token.WithLeadingTrivia(token.LeadingTrivia.RemoveAt(index));
			}

			return token;
		}
	}
}
