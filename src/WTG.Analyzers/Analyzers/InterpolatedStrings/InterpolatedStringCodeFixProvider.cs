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

			var interpolatedStringExpression = (InterpolatedStringExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			// the lack of implicit conversion of int -> bool makes me sad :(
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

			switch (interpolatedStringExpression.Contents.First().Kind())
			{
				case SyntaxKind.InterpolatedStringText:
					var text = ((InterpolatedStringTextSyntax)interpolatedStringExpression.Contents[0]).TextToken.Text;

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								Literal(text))
							.WithLeadingTrivia(interpolatedStringExpression?.GetLeadingTrivia())
							.WithTrailingTrivia(interpolatedStringExpression?.GetTrailingTrivia())));
				case SyntaxKind.Interpolation:
					var variable = ((InterpolationSyntax)interpolatedStringExpression.Contents[0]).Expression;

					// only do this here because it's only necessary here
					var semanticModel = await document.GetSemanticModelAsync(c).ConfigureAwait(true);

					if (semanticModel?.GetTypeInfo(variable, c).Type?.SpecialType == SpecialType.System_String)
					{
						return document.WithSyntaxRoot(
								root.ReplaceNode(
									interpolatedStringExpression,
									variable
									.WithLeadingTrivia(interpolatedStringExpression?.GetLeadingTrivia())
									.WithTrailingTrivia(interpolatedStringExpression?.GetTrailingTrivia())));
					}

					return document.WithSyntaxRoot(
						root.ReplaceNode(
							interpolatedStringExpression,
							InvocationExpression(
								MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								variable,
								IdentifierName("ToString")))
							.WithLeadingTrivia(interpolatedStringExpression?.GetLeadingTrivia())
							.WithTrailingTrivia(interpolatedStringExpression?.GetTrailingTrivia())));
				default:
					return document;
			}
		}
	}
}
