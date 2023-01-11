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
	sealed class InterpolatedStringCodeFixProvider : CodeFixProvider
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

		public static async Task<Document> FixUselessInterpolatedString (Document document, Diagnostic diagnostic, CancellationToken c)
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
							.WithLeadingTrivia(interpolatedStringExpression?.GetLeadingTrivia())
							.WithTrailingTrivia(interpolatedStringExpression?.GetTrailingTrivia())));
			}

			switch (interpolatedStringExpression.Contents[0].Kind())
			{
				case SyntaxKind.InterpolatedStringText:
					var text = ((InterpolatedStringTextSyntax)interpolatedStringExpression.Contents[0]).TextToken.Text;

					// Tried to replace 'Literal(text)' with the straight TextToken and it didn't work?

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								Literal(text))
							.WithTriviaFrom(interpolatedStringExpression)));

				case SyntaxKind.Interpolation:
					var interpolation = ((InterpolationSyntax)interpolatedStringExpression.Contents[0]).Expression;

					var semanticModel = await document.RequireSemanticModelAsync(c).ConfigureAwait(true);

					if (semanticModel.GetTypeInfo(interpolation, c).Type?.SpecialType == SpecialType.System_String)
					{
						return document.WithSyntaxRoot(
								root.ReplaceNode(
									interpolatedStringExpression,
									interpolation
									.WithTriviaFrom(interpolatedStringExpression)));
					}

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							InvocationExpression(
								MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								interpolation,
								IdentifierName("ToString")))
							.WithTriviaFrom(interpolatedStringExpression)));
				default:
					return document;
			}
		}
	}
}
