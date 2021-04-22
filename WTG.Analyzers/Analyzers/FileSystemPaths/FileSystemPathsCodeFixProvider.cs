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

			return document;
		}
	}
}
