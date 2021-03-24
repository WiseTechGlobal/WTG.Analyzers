using System;
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Analyzers;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringBuilderAppendCodeFixProvider))]
	[Shared]
	public sealed class StringBuilderAppendCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontMutateAppendedStringArgumentsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var document = context.Document;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Convert to use appends.",
					createChangedDocument: c => ConvertToAppendsAsync(diagnostic, document, c),
					equivalenceKey: "ConvertToAppends"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> ConvertToAppendsAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var invocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
			var memberExpression = (MemberAccessExpressionSyntax)invocation.Expression;
			var mode = GetMode(diagnostic);

			var firstArgument = invocation.ArgumentList.Arguments[0].Expression;

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invocation,
					Translate(memberExpression.Expression.WithoutTrailingTrivia(), firstArgument, mode)
						.WithTrailingTrivia(invocation.GetTrailingTrivia())));
		}

		static StringBuilderAppendMode GetMode(Diagnostic diagnostic)
		{
			if (diagnostic.Properties.TryGetValue(nameof(StringBuilderAppendMode), out var value) &&
				Enum.TryParse(value, out StringBuilderAppendMode mode))
			{
				return mode;
			}

			return StringBuilderAppendMode.Append;
		}

		static ExpressionSyntax Translate(ExpressionSyntax baseExpression, ExpressionSyntax valueExpression, StringBuilderAppendMode mode)
		{
			if (valueExpression.IsKind(SyntaxKind.AddExpression))
			{
				var binaryExpression = (BinaryExpressionSyntax)valueExpression;

				return Translate(
					Translate(
						baseExpression,
						binaryExpression.Left,
						StringBuilderAppendMode.Append),
					binaryExpression.Right,
					mode);
			}

			switch (mode)
			{
				case StringBuilderAppendMode.Append:
					return CreateAppendExpression(baseExpression, valueExpression, false);
				case StringBuilderAppendMode.AppendLine:
					return CreateAppendExpression(baseExpression, valueExpression, true);
				case StringBuilderAppendMode.AppendAppendLine:
					return CreateAppendExpression(CreateAppendExpression(baseExpression, valueExpression, false), null, true);

				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unrecognised mode.");
			}
		}

		static ExpressionSyntax CreateAppendExpression(ExpressionSyntax baseExpression, ExpressionSyntax? valueExpression, bool appendLine)
		{
			return SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					baseExpression,
					appendLine ? AppendLine : Append),
				valueExpression == null
					? SyntaxFactory.ArgumentList(
						SyntaxFactory.SeparatedList<ArgumentSyntax>())
					: SyntaxFactory.ArgumentList(
						SyntaxFactory.SingletonSeparatedList(
							SyntaxFactory.Argument(valueExpression))));
		}

		static readonly IdentifierNameSyntax Append = SyntaxFactory.IdentifierName(nameof(StringBuilder.Append));
		static readonly IdentifierNameSyntax AppendLine = SyntaxFactory.IdentifierName(nameof(StringBuilder.AppendLine));
	}
}
