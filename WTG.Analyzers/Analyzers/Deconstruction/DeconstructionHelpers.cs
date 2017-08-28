using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class DeconstructionHelpers
	{
		public static DeclarationExpressionSyntax UnifyVarsInTupleExpression(TupleExpressionSyntax tupleExpression)
		{
			var declarationExpression = SyntaxFactory.DeclarationExpression(
				SyntaxFactory.IdentifierName(
					SyntaxFactory.Identifier(
						SyntaxFactory.TriviaList(),
						"var",
						SyntaxFactory.TriviaList(SyntaxFactory.Space))),
				(VariableDesignationSyntax)TupleExpressionVarStripper.Instance.Visit(tupleExpression))
				.WithTriviaFrom(tupleExpression);

			return declarationExpression;
		}

		sealed class TupleExpressionVarStripper : CSharpSyntaxRewriter
		{
			TupleExpressionVarStripper()
				: base(visitIntoStructuredTrivia: false)
			{
			}

			public static TupleExpressionVarStripper Instance { get; } = new TupleExpressionVarStripper();

			public override SyntaxNode VisitTupleExpression(TupleExpressionSyntax node)
			{
				var args = node.Arguments.Select(x => Visit(x.Expression));
				var variables = SyntaxFactory.SeparatedList(args);
				var newNode = SyntaxFactory.ParenthesizedVariableDesignation(variables);
				return newNode;
			}

			public override SyntaxNode VisitDeclarationExpression(DeclarationExpressionSyntax node) => node.Designation;
		}
	}
}
