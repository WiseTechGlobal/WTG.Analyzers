using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class DebuggerDisplayContext
	{
		public static DebuggerDisplayContext Create(SemanticModel model, AttributeSyntax attribute)
		{
			var pos = ContextLocator.Instance.Visit(attribute);
			var newModel = model.Compilation.GetSemanticModel(model.SyntaxTree, true);
			return new DebuggerDisplayContext(newModel, pos);
		}

		public TypeInfo GetExpressionTypeInfo(ExpressionSyntax expression)
		{
			return model.GetSpeculativeTypeInfo(pos, expression, SpeculativeBindingOption.BindAsExpression);
		}

		public SymbolInfo GetExpressionSymbolInfo(ExpressionSyntax expression)
		{
			return model.GetSpeculativeSymbolInfo(pos, expression, SpeculativeBindingOption.BindAsExpression);
		}

		DebuggerDisplayContext(SemanticModel model, int pos)
		{
			this.model = model;
			this.pos = pos;
		}

		readonly SemanticModel model;
		readonly int pos;

		sealed class ContextLocator : CSharpSyntaxVisitor<int>
		{
			public static ContextLocator Instance { get; } = new ContextLocator();

			ContextLocator()
			{
			}

			public override int DefaultVisit(SyntaxNode node) => node.GetFirstToken().SpanStart;

			public override int VisitAttribute(AttributeSyntax node) => Visit(node.Parent);
			public override int VisitAttributeList(AttributeListSyntax node) => Visit(node.Parent);

			public override int VisitClassDeclaration(ClassDeclarationSyntax node) => node.OpenBraceToken.SpanStart;
			public override int VisitStructDeclaration(StructDeclarationSyntax node) => node.OpenBraceToken.SpanStart;
		}
	}
}
