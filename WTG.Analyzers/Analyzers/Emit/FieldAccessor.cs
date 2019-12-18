using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class FieldAccessor : CSharpSyntaxVisitor<SimpleNameSyntax>
	{
		public static FieldAccessor Instance { get; } = new FieldAccessor();

		FieldAccessor()
		{
		}

		public override SimpleNameSyntax VisitIdentifierName(IdentifierNameSyntax node) => node;
		public override SimpleNameSyntax VisitArgument(ArgumentSyntax node) => node.Expression.Accept(this);
		public override SimpleNameSyntax VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression.Accept(this);
		public override SimpleNameSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => node.Name;
	}
}
