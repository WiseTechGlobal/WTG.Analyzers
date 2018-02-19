using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WhitespaceCodeFixProvider))]
	[Shared]
	public sealed class WhitespaceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotLeaveWhitespaceOnTheEndOfTheLineDiagnosticID,
			Rules.IndentWithTabsRatherThanSpacesDiagnosticID,
			Rules.UseConsistentLineEndingsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.DoNotLeaveWhitespaceOnTheEndOfTheLineDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Remove trailing whitespace.",
							createChangedDocument: c => FixTrailingWhitespace(context.Document, diagnostic, c),
							equivalenceKey: "RemoveTrailingWhitespace"),
						diagnostic);
					break;

				case Rules.IndentWithTabsRatherThanSpacesDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Replace spaces with tabs.",
							createChangedDocument: c => FixLeadingWhitespace(context.Document, diagnostic, c),
							equivalenceKey: "ReplaceSpacesWithTabs"),
						diagnostic);
					break;

				case Rules.UseConsistentLineEndingsDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Replace with CRLF.",
							createChangedDocument: c => FixLineEndings(context.Document, diagnostic, c),
							equivalenceKey: "FixLineEnding"),
						diagnostic);
					break;
			}

			return Task.FromResult<object>(null);
		}

		static async Task<Document> FixTrailingWhitespace(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var trivia = FindTrivia(root, diagnostic.Location);
			var token = trivia.Token;

			return document.WithSyntaxRoot(
				root.ReplaceToken(
					token,
					RemoveTrivia(token, trivia)));
		}

		static async Task<Document> FixLeadingWhitespace(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var triviaList = new SyntaxTrivia[diagnostic.AdditionalLocations.Count + 1];
			triviaList[0] = FindTrivia(root, diagnostic.Location);

			for (var i = 0; i < diagnostic.AdditionalLocations.Count; i++)
			{
				triviaList[i + 1] = FindTrivia(root, diagnostic.AdditionalLocations[i]);
			}

			return document.WithSyntaxRoot(
				root.ReplaceTrivia(
					triviaList,
					RewriteIndentingUsingTabs));
		}

		static async Task<Document> FixLineEndings(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var trivia = FindTrivia(root, diagnostic.Location);

			return document.WithSyntaxRoot(
				root.ReplaceTrivia(
					trivia,
					SyntaxFactory.CarriageReturnLineFeed));
		}

		static SyntaxTrivia FindTrivia(SyntaxNode root, Location location)
		{
			var diagnosticSpan = location.SourceSpan;
			var trivia = root.FindTrivia(diagnosticSpan.Start, findInsideTrivia: true);
			return trivia;
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

		static SyntaxTrivia RewriteIndentingUsingTabs(SyntaxTrivia originalTrivia, SyntaxTrivia targetTrivia)
		{
			switch (originalTrivia.Kind())
			{
				case SyntaxKind.WhitespaceTrivia:
					return ModifyWhitespaceTrivia(originalTrivia);

				case SyntaxKind.DocumentationCommentExteriorTrivia:
					return ModifyDocumentationCommentExteriorTrivia(originalTrivia);

				default:
					return targetTrivia;
			}

			SyntaxTrivia ModifyWhitespaceTrivia(SyntaxTrivia trivia)
			{
				var (column, _) = CalculateColumn(trivia.ToString());
				return SyntaxFactory.Whitespace(StringFromIndentColumn(column));
			}

			SyntaxTrivia ModifyDocumentationCommentExteriorTrivia(SyntaxTrivia trivia)
			{
				var originalText = trivia.ToString();
				var (column, length) = CalculateColumn(originalText);

				if (length == originalText.Length)
				{
					return SyntaxFactory.DocumentationCommentExterior(StringFromIndentColumn(column));
				}

				var builder = new StringBuilder();
				builder.Append('\t', column / AssumedTabSize);
				builder.Append(' ', column % AssumedTabSize);
				builder.Append(originalText, length, originalText.Length - length);
				return SyntaxFactory.DocumentationCommentExterior(builder.ToString());
			}
		}

		static (int column, int length) CalculateColumn(string text)
		{
			var column = 0;

			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '\t')
				{
					// Round up to the nearest multiple of AssumedTabSize.
					column = (column + AssumedTabSize);
					column = column - (column % AssumedTabSize);
				}
				else if (char.IsWhiteSpace(text, i))
				{
					column++;
				}
				else
				{
					return (column, i);
				}
			}

			return (column, text.Length);
		}

		static string StringFromIndentColumn(int column)
		{
			var tabCount = column / AssumedTabSize;
			var spaceCount = column % AssumedTabSize;

			var tabString = tabCount < cachedTabStrings.Length ? cachedTabStrings[tabCount] : new string('\t', tabCount);

			if (spaceCount == 0)
			{
				return tabString;
			}

			return tabString + cachedSpaceStrings[spaceCount];
		}

		const int AssumedTabSize = 4;

		static string[] cachedTabStrings =
		{
			string.Empty,
			"\t",
			"\t\t",
			"\t\t\t",
			"\t\t\t\t",
			"\t\t\t\t\t",
			"\t\t\t\t\t\t",
		};

		static string[] cachedSpaceStrings = new string[AssumedTabSize]
		{
			string.Empty,
			" ",
			"  ",
			"   ",
		};
	}
}
