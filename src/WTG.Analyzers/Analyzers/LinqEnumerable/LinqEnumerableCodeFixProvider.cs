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

			string equivalenceKey, title;

			switch (diagnostic.Id)
			{
				case Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID:
					title = "Replace Concat with Append";
					equivalenceKey = "Append";
					break;

				case Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID:
					title = "Replace Concat with Prepend";
					equivalenceKey = "Prepend";
					break;

				case Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID:
					title = "Merge collections";
					equivalenceKey = "Join";
					break;

				default:
					return Task.CompletedTask;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: c => ReplaceWithAppropriateMethod(context.Document, diagnostic, c),
					equivalenceKey),
				diagnostic);

			return Task.CompletedTask;
		}

		public static async Task<Document> ReplaceWithAppropriateMethod(Document document, Diagnostic diagnostic, CancellationToken c)
		{
			var root = await document.RequireSyntaxRootAsync(c).ConfigureAwait(true);
			var semanticModel = await document.RequireSemanticModelAsync(c).ConfigureAwait(true);

			var memberAccessExpression = (MemberAccessExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			if (memberAccessExpression.Parent == null)
			{
				return document;
			}

			var newNode = FixMemberAccessExpression(memberAccessExpression, diagnostic, semanticModel);

			if (newNode == null)
			{
				return document;
			}

			return document.WithSyntaxRoot(root.ReplaceNode(
				memberAccessExpression.Parent, newNode));
		}

		public static SyntaxNode? FixMemberAccessExpression(MemberAccessExpressionSyntax m, Diagnostic d, SemanticModel semanticModel)
		{
			return d.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => FixConcatWithAppendMethod(m, semanticModel),
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => FixConcatWithPrependMethod(m, semanticModel),
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => FixConcatWithNewCollection(m),
				_ => null,
			};
		}

		public static SyntaxNode FixConcatWithAppendMethod(MemberAccessExpressionSyntax m, SemanticModel semanticModel)
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
					GetMethodName(nameof(Enumerable.Append), invocation, semanticModel)
						.WithTriviaFrom(m.Name)))
				.WithArgumentList(
					ArgumentList(
						SeparatedList<ArgumentSyntax>(listOfArgumentsAndSeparators)))
				.WithTriviaFrom(invocation);
		}

		public static SyntaxNode? FixConcatWithPrependMethod(MemberAccessExpressionSyntax m, SemanticModel semanticModel)
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
						.WithTriviaFrom(m.Expression)
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
					GetMethodName(nameof(Enumerable.Prepend), invocation, semanticModel)
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

		static SimpleNameSyntax GetMethodName(string methodName, InvocationExpressionSyntax invocation, SemanticModel semanticModel)
		{
			if (NeedsExplicitTypeArgument(invocation, semanticModel, out var typeArgument))
			{
				return GenericName(Identifier(methodName))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList(typeArgument)));
			}

			return IdentifierName(methodName);
		}

		static bool NeedsExplicitTypeArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel, out TypeSyntax typeArgument)
		{
			var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

			if (methodSymbol != null && methodSymbol.TypeArguments.Length == 1)
			{
				var concatTypeArg = methodSymbol.TypeArguments[0];
				var elementExpression = GetElementExpression(invocation);

				if (elementExpression != null)
				{
					var elementType = semanticModel.GetTypeInfo(elementExpression).Type;

					if (elementType != null && !SymbolEqualityComparer.Default.Equals(elementType, concatTypeArg))
					{
						typeArgument = ParseTypeName(concatTypeArg.ToMinimalDisplayString(semanticModel, invocation.SpanStart));
						return true;
					}
				}
			}

			typeArgument = null!;
			return false;
		}

		static ExpressionSyntax? GetElementExpression(InvocationExpressionSyntax invocation)
		{
			var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

			if (memberAccess == null)
			{
				return null;
			}

			var arguments = invocation.ArgumentList.Arguments;

			if (arguments.Count == 1)
			{
				// Extension method style: collection.Concat(enumerable) or enumerable.Concat(collection)
				// For Prepend: new T[] { element }.Concat(enumerable) - element is in m.Expression
				// For Append: enumerable.Concat(new T[] { element }) - element is in arguments[0]
				var receiverExpr = memberAccess.Expression.TryGetExpressionFromParenthesizedExpression();
				var argExpr = arguments[0].Expression.TryGetExpressionFromParenthesizedExpression();

				// Check which one is the single-element collection
				var receiverFirstValue = LinqEnumerableUtils.GetFirstValue(receiverExpr);
				if (receiverFirstValue != null)
				{
					return receiverFirstValue;
				}

				var argFirstValue = LinqEnumerableUtils.GetFirstValue(argExpr);
				if (argFirstValue != null)
				{
					return argFirstValue;
				}
			}
			else if (arguments.Count == 2)
			{
				// Static method style: Enumerable.Concat(collection, enumerable)
				var arg0Expr = arguments[0].Expression.TryGetExpressionFromParenthesizedExpression();
				var arg1Expr = arguments[1].Expression.TryGetExpressionFromParenthesizedExpression();

				var arg0FirstValue = LinqEnumerableUtils.GetFirstValue(arg0Expr);
				if (arg0FirstValue != null)
				{
					return arg0FirstValue;
				}

				var arg1FirstValue = LinqEnumerableUtils.GetFirstValue(arg1Expr);
				if (arg1FirstValue != null)
				{
					return arg1FirstValue;
				}
			}

			return null;
		}
	}
}
