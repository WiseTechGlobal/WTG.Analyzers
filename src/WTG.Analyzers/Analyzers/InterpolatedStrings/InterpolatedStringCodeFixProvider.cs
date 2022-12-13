using System;
using System.Collections.Immutable;
using System.Composition;
using System.Dynamic;
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
	public class InterpolatedStringCodeFixProvider : CodeFixProvider
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
			var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(true);

			if (root == null)
			{
				return document;
			}

			var interpolatedStrings = from m in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>()
									  where m.GetLocation() == diagnostic.Location
									  select m;

			var interpolatedStringExpression = (InterpolatedStringExpressionSyntax)interpolatedStrings.FirstOrDefault();

			switch (interpolatedStringExpression.Contents.First().Kind())
			{
				case SyntaxKind.InterpolatedStringText:
					var text = ((InterpolatedStringTextSyntax)interpolatedStringExpression.Contents.First()).TextToken.Text;

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								Literal(text))
							.WithLeadingTrivia(interpolatedStringExpression!.GetLeadingTrivia())
							.WithTrailingTrivia(interpolatedStringExpression!.GetTrailingTrivia())));
				case SyntaxKind.Interpolation:
					var variable = ((InterpolationSyntax)interpolatedStringExpression.Contents.First()).Expression.ToString();

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							InvocationExpression(
								MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								IdentifierName(variable),
								IdentifierName("ToString")))
							.WithLeadingTrivia(interpolatedStringExpression!.GetLeadingTrivia())
							.WithTrailingTrivia(interpolatedStringExpression!.GetTrailingTrivia())));
				default:
					return document;
			}
		}
	}
}
