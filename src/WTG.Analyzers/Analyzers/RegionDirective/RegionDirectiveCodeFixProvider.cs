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

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegionDirectiveCodeFixProvider))]
	[Shared]
	public sealed class RegionDirectiveCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotNestRegionsDiagnosticID,
			Rules.RegionsShouldNotSplitStructuresDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var document = context.Document;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Remove Region",
					createChangedDocument: c => RemoveRegionFixAsync(diagnostic, document, c),
					equivalenceKey: "RemoveRegion"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveRegionFixAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
		{
			// Directives are structured trivia and encompass everything up to (and including) the following newline;
			// This newline will be considered trivia of the inner structure rather than sibling trivia to the directive trivia,
			// so we don't need to look for the next EOL, only leading whitespace. This is made simplier as the trailing/leading
			// trivia split is typically made just after a newline, so the leading whitespace should be in the same trivia list.

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var startTrivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start);
			var endTrivia = root.FindTrivia(diagnostic.AdditionalLocations[0].SourceSpan.Start);

			var startToken = startTrivia.Token;
			var endToken = endTrivia.Token;

			if (startToken == endToken)
			{
				var newToken = WithoutTrivia(WithoutTrivia(startToken, startTrivia), endTrivia);
				return document.WithSyntaxRoot(
					root.ReplaceToken(startToken, newToken));
			}
			else
			{
				var newStartToken = WithoutTrivia(startToken, startTrivia);
				var newEndToken = WithoutTrivia(endToken, endTrivia);

				return document.WithSyntaxRoot(
					root.ReplaceTokens(
						new[] { startToken, endToken },
						(original, current) =>
						{
							return (original == startToken) ? newStartToken : newEndToken;
						}));
			}
		}

		static SyntaxToken WithoutTrivia(SyntaxToken token, SyntaxTrivia trivia)
		{
			if (trivia.SpanStart < token.SpanStart)
			{
				return token.WithLeadingTrivia(RemoveTriviaLine(token.LeadingTrivia, trivia));
			}
			else
			{
				return token.WithTrailingTrivia(RemoveTriviaLine(token.TrailingTrivia, trivia));
			}
		}

		static SyntaxTriviaList RemoveTriviaLine(SyntaxTriviaList list, SyntaxTrivia trivia)
		{
			var index = list.IndexOf(trivia);

			if (index < 0)
			{
				return list;
			}

			var start = index;

			while (start > 0)
			{
				var kind = list[start - 1].Kind();

				if (kind == SyntaxKind.EndOfLineTrivia)
				{
					break;
				}
				else if (kind != SyntaxKind.WhitespaceTrivia)
				{
					return list.RemoveAt(index);
				}

				start--;
			}

			return RemoveRange(list, start, index - start + 1);
		}

		static SyntaxTriviaList RemoveRange(SyntaxTriviaList list, int index, int count)
		{
			if (count == list.Count)
			{
				return SyntaxTriviaList.Empty;
			}
			else if (count == 1)
			{
				return list.RemoveAt(index);
			}

			// Why can't they just give us a damn RemoveRange, or even a builder class?
			return SyntaxTriviaList.Empty.AddRange(WithoutRange(list, index, count));
		}

		static IEnumerable<SyntaxTrivia> WithoutRange(SyntaxTriviaList list, int index, int count)
		{
			for (var i = 0; i < index; i++)
			{
				yield return list[i];
			}

			for (var i = index + count; i < list.Count; i++)
			{
				yield return list[i];
			}
		}
	}
}
