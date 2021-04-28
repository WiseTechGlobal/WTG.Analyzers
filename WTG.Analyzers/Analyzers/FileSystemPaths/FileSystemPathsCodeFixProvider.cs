using System;
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileSystemPathsCodeFixProvider))]
	[Shared]
	public sealed class FileSystemPathsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUsePathSeparatorsInPathLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Use Path.Combine(...)",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "UsePathCombine"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static readonly char[] PathSeparatorChars = new[] { '/', '\\' };

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (ExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var argumentSyntax = (ArgumentSyntax)node.Parent;
			var argumentList = (ArgumentListSyntax)argumentSyntax.Parent;
			var invocationExpression = (InvocationExpressionSyntax)argumentList.Parent;

			if (node.IsKind(SyntaxKind.StringLiteralExpression))
			{
				var literal = (LiteralExpressionSyntax)node;
				return await FixStringLiteralExpression(document, literal, cancellationToken).ConfigureAwait(false);
			}
			else if (node.IsKind(SyntaxKind.InterpolatedStringExpression))
			{
				var interpolatedString = (InterpolatedStringExpressionSyntax)node;
				return await FixInterpolatedStringExpression(document, interpolatedString, cancellationToken).ConfigureAwait(false);
			}

			return document;
		}

		static async Task<Document> FixStringLiteralExpression(Document document, LiteralExpressionSyntax literal, CancellationToken cancellationToken)
		{
			var root = await literal.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var argumentSyntax = (ArgumentSyntax)literal.Parent;
			var argumentList = (ArgumentListSyntax)argumentSyntax.Parent;

			var literalValue = literal.Token.ValueText;
			var newLiterals = literalValue.Split(PathSeparatorChars, StringSplitOptions.RemoveEmptyEntries);

			var arguments = new List<ArgumentSyntax>(capacity: argumentList.Arguments.Count - 1 + newLiterals.Length);

			var indexOfLiteral = argumentList.Arguments.IndexOf(argumentSyntax);
			arguments.AddRange(argumentList.Arguments.Take(indexOfLiteral));

			for (var i = 0; i < newLiterals.Length; i++)
			{
				var expression = SyntaxFactory.LiteralExpression(
					SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal(newLiterals[i]));

				if (i == 0)
				{
					expression = expression.WithLeadingTrivia(literal.GetLeadingTrivia());
				}

				if (i + 1 == newLiterals.Length)
				{
					expression = expression.WithTrailingTrivia(literal.GetTrailingTrivia());
				}

				var argument = SyntaxFactory.Argument(expression);
				arguments.Add(argument);
			}

			arguments.AddRange(argumentList.Arguments.Skip(indexOfLiteral + 1));
			var newArgumentList = SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList(arguments))
				.WithTriviaFrom(argumentList);

			return document.WithSyntaxRoot(
				root.ReplaceNode(argumentList, newArgumentList));
		}

		static async Task<Document> FixInterpolatedStringExpression(Document document, InterpolatedStringExpressionSyntax literal, CancellationToken cancellationToken)
		{
			var root = await literal.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var argumentSyntax = (ArgumentSyntax)literal.Parent;
			var argumentList = (ArgumentListSyntax)argumentSyntax.Parent;

			var arguments = new List<ArgumentSyntax>(capacity: argumentList.Arguments.Count + 1);

			var indexOfLiteral = argumentList.Arguments.IndexOf(argumentSyntax);
			arguments.AddRange(argumentList.Arguments.Take(indexOfLiteral));

			var components = new List<InterpolatedStringContentSyntax>();
			foreach (var component in literal.Contents)
			{
				if (!component.IsKind(SyntaxKind.InterpolatedStringText))
				{
					components.Add(component);
					continue;
				}

				var textSyntax = (InterpolatedStringTextSyntax)component;

				if (textSyntax.TextToken.Text.IndexOfAny(PathSeparatorChars) >= 0)
				{
					if (components.Count > 0)
					{
						// Write out what we've already discovered
						arguments.Add(
							MakeArgumentFromInterpolatedStringComponents(
								literal.StringStartToken,
								components));
						components.Clear();
					}

					var pieces = textSyntax.TextToken.ValueText.Split(PathSeparatorChars);
					if (pieces[0].Length > 0)
					{
						arguments.Add(
							SyntaxFactory.Argument(
								SyntaxFactory.LiteralExpression(
									SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(pieces[0])
									.WithLeadingTrivia(textSyntax.GetLeadingTrivia()))));
					}

					for (var i = 1; i < (pieces.Length - 1); i++)
					{
						arguments.Add(
							SyntaxFactory.Argument(
								SyntaxFactory.LiteralExpression(
									SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(pieces[i]))));
					}

					if (pieces.Length > 1 && pieces[pieces.Length - 1].Length > 0)
					{
						arguments.Add(
							SyntaxFactory.Argument(
								SyntaxFactory.LiteralExpression(
									SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(pieces[pieces.Length - 1])
									.WithTrailingTrivia(textSyntax.GetTrailingTrivia()))));
					}
				}
			}

			if (components.Count > 0)
			{
				// Write out any trailing components as the final argument;
				arguments.Add(
					MakeArgumentFromInterpolatedStringComponents(
						literal.StringStartToken,
						components));
			}

			arguments.AddRange(argumentList.Arguments.Skip(indexOfLiteral + 1));
			var newArgumentList = SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList(arguments))
				.WithTriviaFrom(argumentList);

			return document.WithSyntaxRoot(
				root.ReplaceNode(argumentList, newArgumentList));
		}

		static ArgumentSyntax MakeArgumentFromInterpolatedStringComponents(SyntaxToken startToken, IList<InterpolatedStringContentSyntax> contents)
		{
			if (contents.Count == 1)
			{
				// We have an opportunity here to simplify the expression.
				switch (contents[0].Kind())
				{
					case SyntaxKind.InterpolatedStringText:
						var text = (InterpolatedStringTextSyntax)contents[0];

						return SyntaxFactory.Argument(
							SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(text.TextToken.ValueText)));

					case SyntaxKind.Interpolation:
						var interpolation = (InterpolationSyntax)contents[0];
						if (interpolation.FormatClause is null && interpolation.AlignmentClause is null)
						{
							return SyntaxFactory.Argument(interpolation.Expression);
						}
						break;
				}
			}

			return SyntaxFactory.Argument(
				SyntaxFactory.InterpolatedStringExpression(
					startToken,
					SyntaxFactory.List(contents)));
		}
	}
}
