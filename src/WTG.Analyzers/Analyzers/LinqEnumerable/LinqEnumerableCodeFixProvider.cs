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
using WTG.Analyzers.Analyzers.LinqEnumerable;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LinqEnumerableCodeFixProvider))]
	[Shared]
	public class LinqEnumerableCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID,
			Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID,
			Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			var equivalenceKey = diagnostic.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => "Append",
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => "Prepend",
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => "Join",
				_ => null,
			};

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Fix incorrect use of .Concat",
					createChangedDocument: c => ReplaceWithAppropriateMethod(context.Document, diagnostic, c),
					equivalenceKey: equivalenceKey),
				diagnostic);

			return Task.CompletedTask;
		}

		public static async Task<Document> ReplaceWithAppropriateMethod(Document document, Diagnostic diagnostic, CancellationToken c)
		{
			var root = await document.RequireSyntaxRootAsync(c).ConfigureAwait(true);

			var memberAccessExpression = (MemberAccessExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			if (memberAccessExpression.Parent == null)
			{
				return document;
			}

			var newNode = FixMemberAccessExpression(memberAccessExpression, diagnostic);

			if (newNode == null)
			{
				return document;
			}

			return document.WithSyntaxRoot(root.ReplaceNode(
				memberAccessExpression.Parent, newNode));
		}

		public static SyntaxNode? FixMemberAccessExpression(MemberAccessExpressionSyntax m, Diagnostic d)
		{
			return d.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => FixConcatWithAppendMethod(m),
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => FixConcatWithPrependMethod(m),
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => FixConcatWithNewCollection(m),
				_ => null,
			};
		}

		public static SyntaxNode FixConcatWithAppendMethod(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			var listOfArgumentsAndSeparators = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[0].Expression)!));
					break;
				case 2:
					listOfArgumentsAndSeparators.Add(invocation.ArgumentList.Arguments[0]);
					listOfArgumentsAndSeparators.Add(Token(SyntaxKind.CommaToken));
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[1].Expression)!));
					break;
			}

			return InvocationExpression(
					MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						IdentifierName(m.Expression.ToString()),
						IdentifierName(nameof(Enumerable.Append))))
				.WithArgumentList(
					ArgumentList(
						SeparatedList<ArgumentSyntax>(listOfArgumentsAndSeparators)))
				.WithTriviaFrom(invocation);
		}

		public static SyntaxNode? FixConcatWithPrependMethod(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			if (invocation == null)
			{
				return m;
			}

			var arguments = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).TryGetExpressionFromParenthesizedExpression() : m.Expression;

					arguments.Add(Argument(LinqEnumerableUtils.GetValue(expression)!));

					IdentifierNameSyntax identifier = (IdentifierNameSyntax)invocation.ArgumentList.Arguments[0].Expression;

					return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							identifier,
							IdentifierName(nameof(Enumerable.Prepend))))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithTriviaFrom(invocation);
				case 2:
					var value = LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[0].Expression);

					if (value == null)
					{
						return invocation;
					}

					arguments.Add(invocation.ArgumentList.Arguments[1]);
					arguments.Add(Token(SyntaxKind.CommaToken));
					arguments.Add(Argument(value));

					return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName(nameof(Enumerable)),
							IdentifierName(nameof(Enumerable.Prepend))))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithTriviaFrom(invocation);
				default:
					return invocation;
			}
		}

		public static SyntaxNode FixConcatWithNewCollection(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			if (invocation == null)
			{
				return m;
			}

			LiteralExpressionSyntax a, b;

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).TryGetExpressionFromParenthesizedExpression() : m.Expression;

					a = (LiteralExpressionSyntax)LinqEnumerableUtils.GetValue(expression)!;
					b = (LiteralExpressionSyntax)LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[0].Expression)!;
					break;
				case 2:
					a = (LiteralExpressionSyntax)LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[0].Expression)!;
					b = (LiteralExpressionSyntax)LinqEnumerableUtils.GetValue(invocation.ArgumentList.Arguments[1].Expression)!;
					break;
				default:
					return invocation;
			}

			return ImplicitArrayCreationExpression(
					InitializerExpression(
						SyntaxKind.ArrayInitializerExpression,
						SeparatedList<ExpressionSyntax>(
							new List<SyntaxNodeOrToken>() { a, Token(SyntaxKind.CommaToken), b })))
					.WithTriviaFrom(invocation);
		}
	}
}
