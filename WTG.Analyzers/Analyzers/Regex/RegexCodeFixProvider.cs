using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegexCodeFixProvider))]
	[Shared]
	public sealed class RegexCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Rules.ForbidCompiledInStaticRegexMethodsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					"RemoveCompiledOption",
					c => RemoveCompiledOption(context.Document, diagnostic, c),
					equivalenceKey: "RemoveCompiledOption"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveCompiledOption(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var member = (MemberAccessExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
			return document.WithSyntaxRoot(new FlagRemover(member).Visit(root));
		}

		sealed class FlagRemover : CSharpSyntaxRewriter
		{
			public FlagRemover(SyntaxNode syntax)
			{
				this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
			}

			public override SyntaxNode Visit(SyntaxNode node)
			{
				if (node == null)
				{
					// Roslyn will occasionally pass null in, and when that happens, the only thing we can do is return null.
					return null!;
				}
				else if (node == syntax)
				{
					return NoneOptions.WithTriviaFrom(node);
				}

				return Compare(node.FullSpan, syntax.FullSpan) == 0
					? base.Visit(node)
					: node;
			}

			public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
			{
				var newArgList = (ArgumentListSyntax)base.VisitArgumentList(node);

				if (newArgList == node || node.Arguments.Count < 0)
				{
					return node;
				}

				var lastIndex = node.Arguments.Count - 1;
				var oldLastArg = node.Arguments[lastIndex];
				var newLastArg = newArgList.Arguments[lastIndex];

				// All overloads with RegexOptions at the end of the argument list have a corresponding
				// overload where that one argument is removed. But overloads where RegexOptions is not
				// the last argument don't have such a corresponding overload.
				if (newLastArg != oldLastArg && newLastArg.Expression.HasAnnotation(Discardable))
				{
					newArgList = newArgList.WithArguments(newArgList.Arguments.RemoveAt(lastIndex));
				}

				return newArgList;
			}

			public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
			{
				var newInner = base.Visit(node.Expression);

				if (newInner == node.Expression)
				{
					return node;
				}
				else if (newInner.HasAnnotation(Discardable))
				{
					return NoneOptions.WithTriviaFrom(node);
				}

				return node
					.WithExpression((ExpressionSyntax)newInner)
					.WithAdditionalAnnotations(Simplifier.Annotation);
			}

			public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
			{
				return (node.Kind()) switch
				{
					SyntaxKind.BitwiseOrExpression => VisitBitwiseOr(node),
					SyntaxKind.BitwiseAndExpression => VisitBitwiseAnd(node),
					_ => Visit(node),
				};
			}

			SyntaxNode VisitBitwiseOr(BinaryExpressionSyntax node)
			{
				var diff = Compare(node.OperatorToken.Span, syntax.FullSpan);

				if (diff > 0)
				{
					var tmp = Visit(node.Left);

					if (tmp.HasAnnotation(Discardable))
					{
						return node.Right.WithTriviaFrom(node);
					}

					return node.WithRight((ExpressionSyntax)tmp);
				}
				else if (diff < 0)
				{
					var tmp = Visit(node.Right);

					if (tmp.HasAnnotation(Discardable))
					{
						return node.Left.WithTriviaFrom(node);
					}

					return node.WithLeft((ExpressionSyntax)tmp);
				}

				return node;
			}

			SyntaxNode VisitBitwiseAnd(BinaryExpressionSyntax node)
			{
				var diff = Compare(node.OperatorToken.Span, syntax.FullSpan);

				if (diff > 0)
				{
					var left = Visit(node.Left);
					return left.HasAnnotation(Discardable)
						? NoneOptions.WithTriviaFrom(node)
						: node.WithLeft((ExpressionSyntax)left);
				}
				else if (diff < 0)
				{
					var right = Visit(node.Right);
					return right.HasAnnotation(Discardable)
						? NoneOptions.WithTriviaFrom(node)
						: node.WithRight((ExpressionSyntax)right);
				}

				return node;
			}

			// returns:
			// -1: if x appears entirly before y.
			//  0: if x and y overlap.
			//  1: if x appears entirely after y.
			static int Compare(TextSpan x, TextSpan y)
			{
				if (x.End <= y.Start)
				{
					return -1;
				}
				else if (y.End <= x.Start)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}

			static readonly SyntaxAnnotation Discardable = new SyntaxAnnotation(nameof(Discardable));

			static readonly ExpressionSyntax NoneOptions =
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								SyntaxFactory.IdentifierName("System"),
								SyntaxFactory.IdentifierName("Text")),
							SyntaxFactory.IdentifierName("RegularExpressions")),
						SyntaxFactory.IdentifierName(nameof(RegexOptions))),
					SyntaxFactory.IdentifierName(nameof(RegexOptions.None)))
				.WithAdditionalAnnotations(Simplifier.Annotation)
				.WithAdditionalAnnotations(Discardable);

			readonly SyntaxNode syntax;
		}
	}
}
