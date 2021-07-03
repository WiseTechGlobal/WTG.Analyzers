using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	partial class AsyncAnalyzer
	{
		sealed partial class IsAsyncVoidMethodVisitor : CSharpSyntaxVisitor<bool?>
		{
			public IsAsyncVoidMethodVisitor(SemanticModel model)
			{
				this.model = model;
			}

			public override bool? VisitMethodDeclaration(MethodDeclarationSyntax node) => IsAsyncVoid(node);
			public override bool? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) => IsAsyncVoid(node);

			public override bool? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) => IsAsyncVoid(node);
			public override bool? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => IsAsyncVoid(node);
			public override bool? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => IsAsyncVoid(node);

			public override bool? VisitClassDeclaration(ClassDeclarationSyntax node) => false;
			public override bool? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => false;
			public override bool? VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node) => false;
			public override bool? VisitDelegateDeclaration(DelegateDeclarationSyntax node) => false;
			public override bool? VisitDestructorDeclaration(DestructorDeclarationSyntax node) => false;
			public override bool? VisitEnumDeclaration(EnumDeclarationSyntax node) => false;
			public override bool? VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node) => false;
			public override bool? VisitEventDeclaration(EventDeclarationSyntax node) => false;
			public override bool? VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => false;
			public override bool? VisitFieldDeclaration(FieldDeclarationSyntax node) => false;
			public override bool? VisitGlobalStatement(GlobalStatementSyntax node) => false;
			public override bool? VisitIncompleteMember(IncompleteMemberSyntax node) => false;
			public override bool? VisitIndexerDeclaration(IndexerDeclarationSyntax node) => false;
			public override bool? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => false;
			public override bool? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) => false;
			public override bool? VisitOperatorDeclaration(OperatorDeclarationSyntax node) => false;
			public override bool? VisitPropertyDeclaration(PropertyDeclarationSyntax node) => false;
			public override bool? VisitStructDeclaration(StructDeclarationSyntax node) => false;

			public override bool? DefaultVisit(SyntaxNode node) => null;

			bool IsAsyncVoid(CSharpSyntaxNode node)
			{
				var owningMethod = (IMethodSymbol)model.GetDeclaredSymbol(node);

				return owningMethod != null
					&& owningMethod.IsAsync
					&& owningMethod.ReturnsVoid;
			}

			bool IsAsyncVoid(AnonymousFunctionExpressionSyntax node)
			{
				if (node.AsyncKeyword.IsKind(SyntaxKind.None))
				{
					return false;
				}

				if (model.GetTypeInfo(node).ConvertedType is INamedTypeSymbol type &&
					type.Kind != SymbolKind.ErrorType)
				{
					var methodSymbol = GetInvokeMethod(type);
					return methodSymbol != null && methodSymbol.ReturnsVoid;
				}

				return false;
			}

			static IMethodSymbol? GetInvokeMethod(INamedTypeSymbol delegateType)
			{
				foreach (var member in delegateType.GetMembers())
				{
					if (member is IMethodSymbol method &&
						method.Name == nameof(Action.Invoke))
					{
						return method;
					}
				}

				return null;
			}

			readonly SemanticModel model;
		}
	}
}
