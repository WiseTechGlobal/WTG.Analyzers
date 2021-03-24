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
					return Invoke(baseExpression, Append, valueExpression);
				case StringBuilderAppendMode.AppendLine:
					return Invoke(baseExpression, AppendLine, valueExpression);
				case StringBuilderAppendMode.AppendAppendLine:
					return Invoke(Invoke(baseExpression, Append, valueExpression), AppendLine);

				case StringBuilderAppendMode.AppendFormatMode:
					return Invoke(baseExpression, AppendFormat, ((InvocationExpressionSyntax)valueExpression).ArgumentList);
				case StringBuilderAppendMode.AppendFormatAppendLineMode:
					return Invoke(Invoke(baseExpression, AppendFormat, ((InvocationExpressionSyntax)valueExpression).ArgumentList), AppendLine);

				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unrecognised mode.");
			}
		}

		static ExpressionSyntax CreateAppendExpression(ExpressionSyntax baseExpression, ExpressionSyntax valueExpression, bool appendLine)
		{
			return Invoke(
				baseExpression,
				appendLine ? AppendLine : Append,
				valueExpression);
		}

		static ExpressionSyntax CreateAppendFormatExpression(ExpressionSyntax baseExpression, ArgumentListSyntax arguments) => Invoke(baseExpression, AppendFormat, arguments);

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method)
			=> Invoke(baseExpression, method, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>()));

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method, ExpressionSyntax argument)
			=> Invoke(baseExpression, method, SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument))));

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method, ArgumentListSyntax arguments)
		{
			return SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					baseExpression,
					method),
				arguments);
		}

		static readonly IdentifierNameSyntax Append = SyntaxFactory.IdentifierName(nameof(StringBuilder.Append));
		static readonly IdentifierNameSyntax AppendLine = SyntaxFactory.IdentifierName(nameof(StringBuilder.AppendLine));
		static readonly IdentifierNameSyntax AppendFormat = SyntaxFactory.IdentifierName(nameof(StringBuilder.AppendFormat));
	}
}
