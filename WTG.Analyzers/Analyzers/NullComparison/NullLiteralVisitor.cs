using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class NullLiteralVisitor : CSharpSyntaxVisitor<bool>
	{
		public static NullLiteralVisitor Instance { get; } = new NullLiteralVisitor();

		NullLiteralVisitor()
		{
		}

		public override bool DefaultVisit(SyntaxNode node) => false;
		public override bool VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => Visit(node.Expression);

		public override bool VisitLiteralExpression(LiteralExpressionSyntax node)
			=> node.Kind() == SyntaxKind.NullLiteralExpression ? true : false;
	}
}
