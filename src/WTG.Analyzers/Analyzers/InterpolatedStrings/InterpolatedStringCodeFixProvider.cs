using System;
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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterpolatedStringCodeFixProvider))]
	[Shared]
	public sealed class InterpolatedStringCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			Rules.InterpolatedStringMustBePurposefulDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.FirstOrDefault();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Fix instance of useless interpolated string",
					createChangedDocument: c => FixUselessInterpolatedString(context.Document, diagnostic, c),
					equivalenceKey: "Useless"),
				diagnostic);

			return Task.CompletedTask;
		}

		public static async Task<Document> FixUselessInterpolatedString(Document document, Diagnostic diagnostic, CancellationToken c)
		{
			var root = await document.RequireSyntaxRootAsync(c).ConfigureAwait(true);

			var interpolatedStringExpression = (InterpolatedStringExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			if (interpolatedStringExpression.Contents.Count == 0)
			{
				return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								Literal(string.Empty))
							.WithTriviaFrom(interpolatedStringExpression)));
			}

			switch (interpolatedStringExpression.Contents[0].Kind())
			{
				case SyntaxKind.InterpolatedStringText:
					var syntax = (InterpolatedStringTextSyntax)interpolatedStringExpression.Contents[0];
					var text = syntax.TextToken;

					SyntaxToken literalToken;

					if (interpolatedStringExpression.StringStartToken.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken))
					{
						literalToken = Literal("@\"" + text.Text + '"', text.ValueText);
					}
					else
					{
						literalToken = Literal('"' + text.Text + '"', text.ValueText);
					}

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								literalToken)
							.WithTriviaFrom(interpolatedStringExpression)));

				case SyntaxKind.Interpolation:
					var interpolation = ((InterpolationSyntax)interpolatedStringExpression.Contents[0]).Expression;

					return document.WithSyntaxRoot(
							root.ReplaceNode(
								interpolatedStringExpression,
								interpolation
								.WithTriviaFrom(interpolatedStringExpression)));
				default:
					return document;
			}
		}
	}
}
