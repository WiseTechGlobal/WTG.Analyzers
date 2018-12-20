using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class ExpressionHelper
	{
		public static SimpleNameSyntax GetMethodName(InvocationExpressionSyntax invoke) => invoke?.Expression?.Accept(MethodNameAccessor.Instance);

		public static bool IsContainedInExpressionTree(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var visitor = new IsContainedInExpressionTreeVisitor(model, cancellationToken);

			for (SyntaxNode node = expression; node != null; node = node.Parent)
			{
				var result = visitor.Visit(node);

				if (result.HasValue)
				{
					return result.Value;
				}
			}

			return false;
		}

		sealed class MethodNameAccessor : CSharpSyntaxVisitor<SimpleNameSyntax>
		{
			public static MethodNameAccessor Instance { get; } = new MethodNameAccessor();

			MethodNameAccessor()
			{
			}

			public override SimpleNameSyntax VisitIdentifierName(IdentifierNameSyntax node) => node;
			public override SimpleNameSyntax VisitGenericName(GenericNameSyntax node) => node;
			public override SimpleNameSyntax VisitAliasQualifiedName(AliasQualifiedNameSyntax node) => node.Name;
			public override SimpleNameSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => node.Name;
			public override SimpleNameSyntax VisitMemberBindingExpression(MemberBindingExpressionSyntax node) => node.Name;
		}

		sealed class IsContainedInExpressionTreeVisitor : CSharpSyntaxVisitor<bool?>
		{
			public IsContainedInExpressionTreeVisitor(SemanticModel model, CancellationToken cancellationToken)
			{
				this.model = model;
				this.cancellationToken = cancellationToken;
			}

			public bool IsContainedInExpressionTree(ExpressionSyntax expression)
			{
				for (SyntaxNode node = expression; node != null; node = node.Parent)
				{
					var result = Visit(node);

					if (result.HasValue)
					{
						return result.Value;
					}
				}

				return false;
			}

			public override bool? VisitAccessorDeclaration(AccessorDeclarationSyntax node) => false;
			public override bool? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) => false;
			public override bool? VisitMethodDeclaration(MethodDeclarationSyntax node) => false;

			public override bool? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => VisitLambdaExpressionSyntax(node);
			public override bool? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => VisitLambdaExpressionSyntax(node);

			public override bool? VisitWhereClause(WhereClauseSyntax node) => IsExpressionFromQueryClauseSymbol(node);
			public override bool? VisitOrdering(OrderingSyntax node) => IsExpressionFromNodeSymbol(node);
			public override bool? VisitLetClause(LetClauseSyntax node) => IsExpressionFromQueryClauseSymbol(node);
			public override bool? VisitGroupClause(GroupClauseSyntax node) => IsExpressionFromNodeSymbol(node);
			public override bool? VisitSelectClause(SelectClauseSyntax node) => IsExpressionFromNodeSymbol(node);

			bool IsExpressionFromQueryClauseSymbol(QueryClauseSyntax node)
			{
				var clauseInfo = model.GetQueryClauseInfo(node, cancellationToken);
				var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
				return method != null && IsExpressionFromLinqMethod(method);
			}

			bool IsExpressionFromNodeSymbol(SyntaxNode node)
			{
				var method = (IMethodSymbol)model.GetSymbolInfo(node).Symbol;
				return method != null && IsExpressionFromLinqMethod(method);
			}

			bool? VisitLambdaExpressionSyntax(LambdaExpressionSyntax node)
			{
				var type = model.GetTypeInfo(node, cancellationToken).ConvertedType;
				return type != null && IsExpression(type);
			}

			static bool IsExpressionFromLinqMethod(IMethodSymbol method)
			{
				return IsExpression(method.Parameters[method.IsStatic ? 1 : 0].Type);
			}

			static bool IsExpression(ITypeSymbol type) => type.IsMatch("System.Linq.Expressions.Expression`1");

			readonly SemanticModel model;
			readonly CancellationToken cancellationToken;
		}
	}
}
