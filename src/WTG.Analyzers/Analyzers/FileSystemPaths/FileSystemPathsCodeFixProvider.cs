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
using WTG.Analyzers.Utils;

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

			if (CommonDiagnosticProperties.CanAutoFix(diagnostic))
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Use Path.Combine(...)",
						createChangedDocument: c => Fix(context.Document, diagnostic, c),
						equivalenceKey: "UsePathCombine"),
					diagnostic: diagnostic);
			}

			return Task.CompletedTask;
		}

		static readonly char[] PathSeparatorChars = new[] { '/', '\\' };

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (ExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			return await FixPathExpressionAsync(document, node, cancellationToken).ConfigureAwait(false);
		}

		static async Task<Document> FixPathExpressionAsync(Document document, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var root = await expression.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			if (expression.Parent.IsKind(SyntaxKind.Argument))
			{
				return FixPathExpressionInArgument(document, root, expression, cancellationToken);
			}
			else if (expression.Parent.IsKind(SyntaxKind.ArrayInitializerExpression))
			{
				return FixPathExpressionInArrayInitializer(document, root, expression, cancellationToken);
			}
			else
			{
				throw new NotSupportedException("Inconsistency error - Code Fix Provider is unable to fix expression provided by Analyzer.");
			}
		}

		static Document FixPathExpressionInArgument(Document document, SyntaxNode root, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var argumentSyntax = (ArgumentSyntax?)expression.Parent;
			NRT.Assert(argumentSyntax != null, "The fixer should only be running on a full and complete document.");
			var argumentList = (ArgumentListSyntax?)argumentSyntax.Parent;
			NRT.Assert(argumentList != null, "The fixer should only be running on a full and complete document.");

			var indexOfLiteral = argumentList.Arguments.IndexOf(argumentSyntax);

			var expressions = SplitPathExpression(expression, cancellationToken);

			var arguments = new List<ArgumentSyntax>(capacity: argumentList.Arguments.Count + expressions.Count - 1);
			arguments.AddRange(argumentList.Arguments.Take(indexOfLiteral));

			for (var i = 0; i < expressions.Count; i++)
			{
				var newExpression = expressions[i];
				var argument = SyntaxFactory.Argument(newExpression);

				if (i == 0)
				{
					argument = argument.WithLeadingTrivia(argumentSyntax.GetLeadingTrivia());
				}

				if (i + 1 == expressions.Count)
				{
					argument = argument.WithTrailingTrivia(argumentSyntax.GetTrailingTrivia());
				}

				arguments.Add(argument);
			}

			arguments.AddRange(argumentList.Arguments.Skip(indexOfLiteral + 1));
			var newArgumentList = SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList(arguments))
				.WithTriviaFrom(argumentList);

			return document.WithSyntaxRoot(
				root.ReplaceNode(argumentList, newArgumentList));
		}

		static Document FixPathExpressionInArrayInitializer(Document document, SyntaxNode root, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var initializer = (InitializerExpressionSyntax?)expression.Parent;
			NRT.Assert(initializer != null, "The fixer should only be running on a full and complete document.");
			var originalExpressions = initializer.Expressions;
			var indexOfExpression = initializer.Expressions.IndexOf(expression);

			var expressions = SplitPathExpression(expression, cancellationToken);

			var newExpressions = new List<ExpressionSyntax>(capacity: originalExpressions.Count + expressions.Count - 1);
			newExpressions.AddRange(originalExpressions.Take(indexOfExpression));
			newExpressions.AddRange(expressions);
			newExpressions.AddRange(originalExpressions.Skip(indexOfExpression + 1));

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					initializer,
					initializer.WithExpressions(
						SyntaxFactory.SeparatedList(newExpressions))));
		}

		static ImmutableList<ExpressionSyntax> SplitPathExpression(ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var expressions = ImmutableList.CreateBuilder<ExpressionSyntax>();

			if (expression.IsKind(SyntaxKind.StringLiteralExpression))
			{
				var literal = (LiteralExpressionSyntax)expression;
				SplitStringLiteralExpression(literal, expressions, cancellationToken);
			}
			else if (expression.IsKind(SyntaxKind.InterpolatedStringExpression))
			{
				var interpolatedString = (InterpolatedStringExpressionSyntax)expression;
				SplitInterpolatedStringExpression(interpolatedString, expressions, cancellationToken);
			}
			else if (expression.IsKind(SyntaxKind.AddExpression))
			{
				var binaryExpression = (BinaryExpressionSyntax)expression;
				SplitAddExpression(binaryExpression, expressions, cancellationToken);
			}
			else
			{
				// Nothing we can do
				expressions.Add(expression);
			}

			return expressions.ToImmutable();
		}

		static void SplitStringLiteralExpression(LiteralExpressionSyntax literal, ImmutableList<ExpressionSyntax>.Builder list, CancellationToken cancellationToken)
		{
			var literalValue = literal.Token.ValueText;
			var newLiterals = literalValue.Split(PathSeparatorChars, StringSplitOptions.RemoveEmptyEntries);

			for (var i = 0; i < newLiterals.Length; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();

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

				list.Add(expression);
			}
		}

		static void SplitInterpolatedStringExpression(InterpolatedStringExpressionSyntax literal, ImmutableList<ExpressionSyntax>.Builder list, CancellationToken cancellationToken)
		{
			var components = new List<InterpolatedStringContentSyntax>();
			foreach (var component in literal.Contents)
			{
				cancellationToken.ThrowIfCancellationRequested();

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
						list.Add(
							MakeExpressionFromInterpolatedStringComponents(
								literal.StringStartToken,
								components));
						components.Clear();
					}

					var pieces = textSyntax.TextToken.ValueText.Split(PathSeparatorChars);
					if (pieces[0].Length > 0)
					{
						list.Add(
							SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(pieces[0])
								.WithLeadingTrivia(textSyntax.GetLeadingTrivia())));
					}

					for (var i = 1; i < (pieces.Length - 1); i++)
					{
						list.Add(
								SyntaxFactory.LiteralExpression(
									SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(pieces[i])));
					}

					if (pieces.Length > 1 && pieces[pieces.Length - 1].Length > 0)
					{
						var text = pieces[pieces.Length - 1];
						components.Add(
							SyntaxFactory.InterpolatedStringText(
								SyntaxFactory.Token(
									SyntaxFactory.TriviaList(),
									SyntaxKind.InterpolatedStringTextToken,
									text,
									text,
									textSyntax.GetTrailingTrivia())));
					}
				}
				else
				{
					components.Add(component);
				}
			}

			if (components.Count > 0)
			{
				// Write out any trailing components as the final argument;
				list.Add(
					MakeExpressionFromInterpolatedStringComponents(
						literal.StringStartToken,
						components));
			}
		}

		static ExpressionSyntax MakeExpressionFromInterpolatedStringComponents(SyntaxToken startToken, IList<InterpolatedStringContentSyntax> contents)
		{
			if (contents.Count == 1)
			{
				// We have an opportunity here to simplify the expression.
				switch (contents[0].Kind())
				{
					case SyntaxKind.InterpolatedStringText:
						var text = (InterpolatedStringTextSyntax)contents[0];

						return SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(text.TextToken.ValueText));

					case SyntaxKind.Interpolation:
						var interpolation = (InterpolationSyntax)contents[0];
						if (interpolation.FormatClause is null && interpolation.AlignmentClause is null)
						{
							return interpolation.Expression;
						}
						break;
				}
			}

			return SyntaxFactory.InterpolatedStringExpression(
					startToken,
					SyntaxFactory.List(contents));
		}

		static void SplitAddExpression(BinaryExpressionSyntax expression, ImmutableList<ExpressionSyntax>.Builder list, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var leftExpressions = SplitPathExpression(expression.Left, cancellationToken);
			var rightExpressions = SplitPathExpression(expression.Right, cancellationToken);

			if (leftExpressions.Count > 1 || rightExpressions.Count > 1)
			{
				for (var i = 0; i < leftExpressions.Count - 1; i++)
				{
					list.Add(leftExpressions[i]);
				}

				list.Add(
					SyntaxFactory.BinaryExpression(
						SyntaxKind.AddExpression,
						leftExpressions[leftExpressions.Count - 1],
						rightExpressions[0]));

				for (var i = 1; i < rightExpressions.Count; i++)
				{
					list.Add(rightExpressions[i]);
				}
			}
			else
			{
				list.AddRange(leftExpressions);
				list.AddRange(rightExpressions);
			}
		}
	}
}
