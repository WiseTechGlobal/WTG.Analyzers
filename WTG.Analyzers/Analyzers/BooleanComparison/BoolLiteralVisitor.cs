using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class BoolLiteralVisitor : CSharpSyntaxVisitor<bool?>
	{
		public static BoolLiteralVisitor Instance { get; } = new BoolLiteralVisitor();

		BoolLiteralVisitor()
		{
		}

		public override bool? DefaultVisit(SyntaxNode node) => null;
		public override bool? VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => Visit(node.Expression);

		public override bool? VisitLiteralExpression(LiteralExpressionSyntax node)
		{
			switch (node.Kind())
			{
				case SyntaxKind.TrueLiteralExpression: return true;
				case SyntaxKind.FalseLiteralExpression: return false;
				default: return null;
			}
		}
	}
}
