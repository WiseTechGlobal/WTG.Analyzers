using System;
using System.Collections.Generic;
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
using WTG.Analyzers.Utils;

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
					Translate(
						await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false),
						memberExpression.Expression.WithoutTrailingTrivia(),
						firstArgument,
						mode,
						cancellationToken)
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

		static ExpressionSyntax Translate(SemanticModel semanticModel, ExpressionSyntax baseExpression, ExpressionSyntax valueExpression, StringBuilderAppendMode mode, CancellationToken cancellationToken)
		{
			if (valueExpression.IsKind(SyntaxKind.AddExpression))
			{
				var binaryExpression = (BinaryExpressionSyntax)valueExpression;

				return Translate(
					semanticModel,
					Translate(
						semanticModel,
						baseExpression,
						binaryExpression.Left,
						StringBuilderAppendMode.Append,
						cancellationToken),
					binaryExpression.Right,
					mode,
					cancellationToken);
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
					return Invoke(baseExpression, AppendFormat, GetFormatArguments(semanticModel, valueExpression, cancellationToken));
				case StringBuilderAppendMode.AppendFormatAppendLineMode:
					return Invoke(Invoke(baseExpression, AppendFormat, GetFormatArguments(semanticModel, valueExpression, cancellationToken)), AppendLine);

				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unrecognised mode.");
			}
		}

		static ArgumentListSyntax GetFormatArguments(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			if (expression.IsKind(SyntaxKind.InvocationExpression))
			{
				return ((InvocationExpressionSyntax)expression).ArgumentList;
			}

			var info = InterpolationInfo.Extract(semanticModel, (InterpolatedStringExpressionSyntax)expression, cancellationToken);
			var arguments = new List<ArgumentSyntax>();
			arguments.Add(SyntaxFactory.Argument(ExpressionSyntaxFactory.CreateLiteral(info.Format)));
			arguments.AddRange(info.Expressions.Select(SyntaxFactory.Argument));
			return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
		}

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
