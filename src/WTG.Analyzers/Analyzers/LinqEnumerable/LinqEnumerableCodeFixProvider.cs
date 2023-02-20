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
using Microsoft.CodeAnalysis.Simplification;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LinqEnumerableCodeFixProvider))]
	[Shared]
	public sealed class LinqEnumerableCodeFixProvider : CodeFixProvider
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
			var invocation = (InvocationExpressionSyntax?)m.Parent;
			NRT.Assert(invocation != null, "MemberAccessExpression should have a parent.");

			var listOfArgumentsAndSeparators = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetFirstValue(invocation.ArgumentList.Arguments[0].Expression)!));
					break;
				case 2:
					listOfArgumentsAndSeparators.Add(invocation.ArgumentList.Arguments[0]);
					listOfArgumentsAndSeparators.Add(Token(SyntaxKind.CommaToken));
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetFirstValue(invocation.ArgumentList.Arguments[1].Expression)!));
					break;
				default:
					throw new InvalidOperationException("Unreachable - Code fix should never trigger for >2 arguments.");
			}

			return InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					ParenthesizedExpression(m.Expression.WithoutTrivia())
						.WithTriviaFrom(m.Expression)
						.WithAdditionalAnnotations(Simplifier.Annotation),
					m.OperatorToken,
					IdentifierName(nameof(Enumerable.Append))
						.WithTriviaFrom(m.Name)))
				.WithArgumentList(
					ArgumentList(
						SeparatedList<ArgumentSyntax>(listOfArgumentsAndSeparators)))
				.WithTriviaFrom(invocation);
		}

		public static SyntaxNode? FixConcatWithPrependMethod(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;
			NRT.Assert(invocation != null, "MemberAccessExpression should have a parent.");

			var listOfArgumentsAndSeparators = new List<SyntaxNodeOrToken>();

			ExpressionSyntax member;

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetFirstValue(m.Expression.TryGetExpressionFromParenthesizedExpression())!));
					member = ParenthesizedExpression(invocation.ArgumentList.Arguments[0].Expression.WithoutTrivia())
						.WithTriviaFrom(invocation)
						.WithAdditionalAnnotations(Simplifier.Annotation);
					break;
				case 2:
					listOfArgumentsAndSeparators.Add(invocation.ArgumentList.Arguments[1]);
					listOfArgumentsAndSeparators.Add(Token(SyntaxKind.CommaToken));
					listOfArgumentsAndSeparators.Add(Argument(LinqEnumerableUtils.GetFirstValue(invocation.ArgumentList.Arguments[0].Expression)!));
					member = m.Expression;
					break;

				default:
					throw new InvalidOperationException("Unreachable - Code fix should never trigger for >2 arguments.");
			}

			return InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					member,
					m.OperatorToken,
					IdentifierName(nameof(Enumerable.Prepend))
						.WithTriviaFrom(m.Name)))
				.WithArgumentList(
					ArgumentList(
						SeparatedList<ArgumentSyntax>(listOfArgumentsAndSeparators)))
				.WithTriviaFrom(invocation);
		}

		public static SyntaxNode FixConcatWithNewCollection(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			if (invocation == null)
			{
				return m;
			}

			var initializerItems = new List<SyntaxNodeOrToken>();
			if (invocation.ArgumentList.Arguments.Count == 1)
			{
				var expression = m.Expression.TryGetExpressionFromParenthesizedExpression();
				foreach (var item in LinqEnumerableUtils.GetValues(expression))
				{
					initializerItems.Add(item);
					initializerItems.Add(Token(SyntaxKind.CommaToken));
				}
			}

			foreach (var argument in invocation.ArgumentList.Arguments)
			{
				var expression = argument.Expression.TryGetExpressionFromParenthesizedExpression();
				foreach (var item in LinqEnumerableUtils.GetValues(expression))
				{
					initializerItems.Add(item);
					initializerItems.Add(Token(SyntaxKind.CommaToken));
				}
			}

			initializerItems.RemoveAt(initializerItems.Count - 1);

			return ImplicitArrayCreationExpression(
					InitializerExpression(
						SyntaxKind.ArrayInitializerExpression,
						SeparatedList<ExpressionSyntax>(initializerItems)))
					.WithTriviaFrom(invocation)
					.WithAdditionalAnnotations(Simplifier.Annotation);
		}
	}
}
