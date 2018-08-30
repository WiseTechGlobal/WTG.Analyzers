using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace WTG.Analyzers.Utils
{
	public static class ExpressionRemover
	{
		public static SyntaxNode ReplaceWithConstantBool(SyntaxNode root, ExpressionSyntax expression, bool value)
			=> ReplaceWithConstantBool(root, ImmutableDictionary<SyntaxNode, bool>.Empty.Add(expression, value));

		public static SyntaxNode ReplaceWithConstantBool(SyntaxNode root, ImmutableDictionary<SyntaxNode, bool> valueMapping)
			=> new ExpressionWriter(valueMapping).Visit(root);

		sealed class ExpressionWriter : CSharpSyntaxRewriter
		{
			public ExpressionWriter(ImmutableDictionary<SyntaxNode, bool> valueMapping)
			{
				this.valueMapping = valueMapping;
			}

			public override SyntaxNode Visit(SyntaxNode node)
			{
				if (node != null && valueMapping.TryGetValue(node, out var value))
				{
					return GetExpression(value).WithTriviaFrom(node);
				}

				return base.Visit(node);
			}

			public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
			{
				var inner = Visit(node.Expression);

				if (CanDiscard(inner))
				{
					return inner.WithTriviaFrom(node);
				}

				return node.WithExpression((ExpressionSyntax)inner);
			}

			public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
			{
				switch (node.Kind())
				{
					case SyntaxKind.LogicalAndExpression:
						return VisitAndOrExpression(node, false);
					case SyntaxKind.LogicalOrExpression:
						return VisitAndOrExpression(node, true);
				}

				return base.VisitBinaryExpression(node);
			}

			public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
			{
				switch (node.Kind())
				{
					case SyntaxKind.LogicalNotExpression:
						return VisitNotExpression(node);
				}

				return base.VisitPrefixUnaryExpression(node);
			}

			public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
			{
				var condition = Visit(node.Condition);

				if (CanDiscard(condition))
				{
					if (condition.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						return Visit(node.WhenTrue).WithTriviaFrom(node);
					}
					else
					{
						return Visit(node.WhenFalse).WithTriviaFrom(node);
					}
				}

				var conditionExpression = (ExpressionSyntax)condition;
				var whenTrue = (ExpressionSyntax)Visit(node.WhenTrue);
				var whenFalse = (ExpressionSyntax)Visit(node.WhenFalse);

				if (CanDiscard(whenTrue))
				{
					if (CanDiscard(whenFalse))
					{
						var newExpression = conditionExpression;

						if (whenTrue.Kind() == whenFalse.Kind())
						{
							newExpression = CoerceTo(newExpression, whenTrue.IsKind(SyntaxKind.TrueLiteralExpression));
						}
						else if (whenTrue.IsKind(SyntaxKind.FalseLiteralExpression))
						{
							newExpression = ExpressionSyntaxFactory.LogicalNot(newExpression);
						}

						return newExpression.WithTriviaFrom(node);
					}

					SyntaxKind opKind;
					SyntaxKind nodeKind;

					if (whenTrue.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						opKind = SyntaxKind.BarBarToken;
						nodeKind = SyntaxKind.LogicalOrExpression;
					}
					else
					{
						opKind = SyntaxKind.AmpersandAmpersandToken;
						nodeKind = SyntaxKind.LogicalAndExpression;
						conditionExpression = ExpressionSyntaxFactory.LogicalNot(conditionExpression);
					}

					return SyntaxFactory.BinaryExpression(
						nodeKind,
						conditionExpression,
						SyntaxFactory.Token(opKind)
							.WithLeadingTrivia(node.QuestionToken.LeadingTrivia)
							.WithTrailingTrivia(node.ColonToken.TrailingTrivia),
						whenFalse);
				}
				else if (CanDiscard(whenFalse))
				{
					SyntaxKind opKind;
					SyntaxKind nodeKind;

					if (whenFalse.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						opKind = SyntaxKind.BarBarToken;
						nodeKind = SyntaxKind.LogicalOrExpression;
						conditionExpression = ExpressionSyntaxFactory.LogicalNot(conditionExpression);
					}
					else
					{
						opKind = SyntaxKind.AmpersandAmpersandToken;
						nodeKind = SyntaxKind.LogicalAndExpression;
					}

					return SyntaxFactory.BinaryExpression(
						nodeKind,
						conditionExpression,
						SyntaxFactory.Token(opKind)
							.WithTriviaFrom(node.QuestionToken),
						whenTrue
							.WithTrailingTrivia(whenFalse.GetTrailingTrivia()));
				}

				return node
					.WithWhenTrue(whenTrue)
					.WithWhenFalse(whenFalse)
					.WithCondition(conditionExpression);
			}

			public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
			{
				var condition = Visit(node.Condition);

				if (CanDiscard(condition))
				{
					if (condition.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						return Visit(node.Statement)
							.WithTriviaFrom(node)
							.WithAdditionalAnnotations(WeakAnnotation);
					}
					else if (node.Else != null)
					{
						return Visit(node.Else.Statement)
							.WithTriviaFrom(node)
							.WithAdditionalAnnotations(WeakAnnotation);
					}
					else
					{
						return EmptyStatement.WithTriviaFrom(node);
					}
				}

				var result = node
					.WithCondition((ExpressionSyntax)condition)
					.WithStatement((StatementSyntax)Visit(node.Statement));

				if (node.Else != null)
				{
					var elseStatement = Visit(node.Else.Statement);

					if (CanDiscard(elseStatement))
					{
						result = result.WithElse(null);
					}
					else
					{
						result = result.WithElse(
							result.Else.WithStatement((StatementSyntax)elseStatement));
					}
				}

				return result.WithTriviaFrom(node);
			}

			public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
			{
				var condition = Visit(node.Condition);

				if (CanDiscard(condition))
				{
					if (condition.IsKind(SyntaxKind.FalseLiteralExpression))
					{
						return EmptyStatement.WithTriviaFrom(node);
					}
				}

				return node
					.WithCondition((ExpressionSyntax)condition)
					.WithStatement((StatementSyntax)Visit(node.Statement))
					.WithTriviaFrom(node);
			}

			public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
			{
				var condition = Visit(node.Condition);

				if (CanDiscard(condition))
				{
					if (condition.IsKind(SyntaxKind.FalseLiteralExpression))
					{
						return Visit(node.Statement)
							.WithTriviaFrom(node)
							.WithAdditionalAnnotations(WeakAnnotation);
					}
				}

				return node
					.WithCondition((ExpressionSyntax)condition)
					.WithStatement((StatementSyntax)Visit(node.Statement))
					.WithTriviaFrom(node);
			}

			public override SyntaxNode VisitBlock(BlockSyntax node)
			{
				var statements = VisitList(node.Statements);
				var modified = false;

				for (var i = 0; i < statements.Count;)
				{
					var current = statements[i];

					if (CanDiscard(current))
					{
						statements = statements.RemoveAt(i);
						modified = true;
					}
					else if (current.IsKind(SyntaxKind.Block) && IsWeak(current))
					{
						statements = InlineBlock(statements, (BlockSyntax)current);
						modified = true;
					}
					else
					{
						i++;
					}
				}

				var result = node.WithStatements(statements);

				if (modified && statements.Count == 0)
				{
					result = result.WithAdditionalAnnotations(DiscardableAnnotation);
				}

				return result;
			}

			static SyntaxList<StatementSyntax> InlineBlock(SyntaxList<StatementSyntax> statements, BlockSyntax block)
			{
				var innerStatements = block.Statements;
				var newStatements = new StatementSyntax[innerStatements.Count];

				for (var i = 0; i < innerStatements.Count; i++)
				{
					newStatements[i] = innerStatements[i]
						.WithAdditionalAnnotations(Formatter.Annotation);
				}

				ref var first = ref newStatements[0];
				first = first.WithLeadingTrivia(
					block.GetLeadingTrivia()
						.Concat(block.OpenBraceToken.TrailingTrivia)
						.Concat(first.GetLeadingTrivia()));

				ref var last = ref newStatements[newStatements.Length - 1];
				last = last.WithTrailingTrivia(
					last.GetTrailingTrivia()
						.Concat(block.CloseBraceToken.LeadingTrivia)
						.Concat(block.GetTrailingTrivia()));

				return statements.ReplaceRange(block, newStatements);
			}

			SyntaxNode VisitAndOrExpression(BinaryExpressionSyntax node, bool shortCircuitValue)
			{
				var left = Visit(node.Left);
				var right = Visit(node.Right);

				if (CanDiscard(left))
				{
					if (left.IsKind(GetExpressionKind(shortCircuitValue)))
					{
						return GetExpression(shortCircuitValue).WithTriviaFrom(node);
					}
					else
					{
						return right.WithLeadingTrivia(node.GetLeadingTrivia());
					}
				}
				else if (CanDiscard(right))
				{
					if (!right.IsKind(GetExpressionKind(shortCircuitValue)))
					{
						return left.WithTrailingTrivia(node.GetTrailingTrivia());
					}

					// We can't remove the LHS even if RHS is the short circuiting kind, as the LHS may have side effects.
				}

				return node
					.WithLeft((ExpressionSyntax)left)
					.WithRight((ExpressionSyntax)right);
			}

			SyntaxNode VisitNotExpression(PrefixUnaryExpressionSyntax node)
			{
				var newInner = Visit(node.Operand);

				if (CanDiscard(newInner))
				{
					if (newInner.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						return FalseExpression.WithTriviaFrom(newInner);
					}
					else
					{
						return TrueExpression.WithTriviaFrom(newInner);
					}
				}

				return node.WithOperand((ExpressionSyntax)newInner);
			}

			readonly ImmutableDictionary<SyntaxNode, bool> valueMapping;
		}

		static ExpressionSyntax CoerceTo(ExpressionSyntax expression, bool result)
		{
			return result ? CoerceToTrue(expression) : CoerceToFalse(expression);
		}
		static ExpressionSyntax CoerceToTrue(ExpressionSyntax expression)
		{
			// expression || true
			// The overall result needs to be true, but we still need to hit the expression incase it has side-effects.
			return SyntaxFactory.BinaryExpression(
				SyntaxKind.LogicalOrExpression,
				expression.WithTrailingTrivia(SyntaxFactory.Space),
				TrueExpression.WithLeadingTrivia(SyntaxFactory.Space));
		}
		static ExpressionSyntax CoerceToFalse(ExpressionSyntax expression)
		{
			// expression && false
			// The overall result needs to be false, but we still need to hit the expression incase it has side-effects.
			return SyntaxFactory.BinaryExpression(
				SyntaxKind.LogicalAndExpression,
				expression.WithTrailingTrivia(SyntaxFactory.Space),
				FalseExpression.WithLeadingTrivia(SyntaxFactory.Space));
		}

		static ExpressionSyntax GetExpression(bool value) => value ? TrueExpression : FalseExpression;
		static SyntaxKind GetExpressionKind(bool value) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;
		static bool CanDiscard(SyntaxNode node) => node.HasAnnotation(DiscardableAnnotation);
		static bool IsWeak(SyntaxNode node) => node.HasAnnotation(WeakAnnotation);

		static readonly SyntaxAnnotation DiscardableAnnotation = new SyntaxAnnotation("WTG.Analyzers:Discardable");
		static readonly SyntaxAnnotation WeakAnnotation = new SyntaxAnnotation("WTG.Analyzers:Weak");
		static readonly LiteralExpressionSyntax TrueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression).WithAdditionalAnnotations(DiscardableAnnotation);
		static readonly LiteralExpressionSyntax FalseExpression = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression).WithAdditionalAnnotations(DiscardableAnnotation);
		static readonly StatementSyntax EmptyStatement = SyntaxFactory.EmptyStatement().WithAdditionalAnnotations(DiscardableAnnotation);
	}
}
