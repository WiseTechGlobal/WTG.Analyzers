using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	static class CodeContractsHelper
	{
		public const string DefaultMessage = "Invalid Argument.";
		public const string NotNullOrEmptyMessage = "Value cannot be null or empty.";

		public static bool IsGenericMethod(InvocationExpressionSyntax invoke, out Location typeLocation)
		{
			var access = (MemberAccessExpressionSyntax)invoke.Expression;

			if (access.Name.IsKind(SyntaxKind.GenericName))
			{
				var name = (GenericNameSyntax)access.Name;
				var typeArgs = name.TypeArgumentList.Arguments;

				if (typeArgs.Count > 0 && !typeArgs[0].IsKind(SyntaxKind.OmittedTypeArgument))
				{
					typeLocation = typeArgs[0].GetLocation();
					return true;
				}
			}

			typeLocation = null;
			return false;
		}

		public static bool IsInPrivateMember(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
		{
			while (node != null)
			{
				switch (node.Kind())
				{
					case SyntaxKind.MethodDeclaration:
						var methodDecl = (MethodDeclarationSyntax)node;
						return methodDecl.ExplicitInterfaceSpecifier == null && IsPrivate(semanticModel.GetDeclaredSymbol(methodDecl));

					case SyntaxKind.ConstructorDeclaration:
						return IsPrivate(semanticModel.GetDeclaredSymbol((ConstructorDeclarationSyntax)node, cancellationToken));

					case SyntaxKind.DestructorDeclaration:
						return IsPrivate(semanticModel.GetDeclaredSymbol((DestructorDeclarationSyntax)node, cancellationToken));

					case SyntaxKind.PropertyDeclaration:
						var propertyDecl = (PropertyDeclarationSyntax)node;
						return propertyDecl.ExplicitInterfaceSpecifier == null && IsPrivate(semanticModel.GetDeclaredSymbol(propertyDecl, cancellationToken));

					case SyntaxKind.EventDeclaration:
						var eventDecl = (EventDeclarationSyntax)node;
						return eventDecl.ExplicitInterfaceSpecifier == null && IsPrivate(semanticModel.GetDeclaredSymbol(eventDecl, cancellationToken));

					case SyntaxKind.AddAccessorDeclaration:
					case SyntaxKind.RemoveAccessorDeclaration:
					case SyntaxKind.GetAccessorDeclaration:
					case SyntaxKind.SetAccessorDeclaration:
					case SyntaxKind.UnknownAccessorDeclaration:
						var accessor = (AccessorDeclarationSyntax)node;
						bool isPrivate = false;

						foreach (var modifier in accessor.Modifiers)
						{
							switch (modifier.Kind())
							{
								case SyntaxKind.PublicKeyword:
								case SyntaxKind.ProtectedKeyword:
								case SyntaxKind.InternalKeyword:
									return false;

								case SyntaxKind.PrivateKeyword:
									isPrivate = true;
									break;
							}
						}

						if (isPrivate)
						{
							return true;
						}
						break;

					case SyntaxKind.NamespaceDeclaration:
					case SyntaxKind.CompilationUnit:
					case SyntaxKind.ClassDeclaration:
					case SyntaxKind.StructDeclaration:
						return false;

					case SyntaxKind.LocalFunctionStatement:
					case SyntaxKind.AnonymousMethodExpression:
					case SyntaxKind.ParenthesizedLambdaExpression:
					case SyntaxKind.SimpleLambdaExpression:
						return true;
				}

				node = node.Parent;
			}

			return false;

			static bool IsPrivate(ISymbol symbol) => symbol != null && symbol.DeclaredAccessibility == Accessibility.Private;
		}

		public static bool IsNullArgumentCheck(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
		{
			var arguments = invoke.ArgumentList.Arguments;

			if (arguments.Count == 0)
			{
				identifierLocation = null;
				return false;
			}

			var comparand = GetComparand(arguments[0].Expression);

			if (comparand == null)
			{
				identifierLocation = null;
				return false;
			}

			var symbol = semanticModel.GetSymbolInfo(comparand, cancellationToken).Symbol;

			if (symbol == null || symbol.Kind != SymbolKind.Parameter)
			{
				identifierLocation = null;
				return false;
			}

			identifierLocation = comparand.GetLocation();
			return true;

			static ExpressionSyntax GetComparand(ExpressionSyntax condition)
			{
				if (!condition.IsKind(SyntaxKind.NotEqualsExpression))
				{
					return null;
				}

				var b = (BinaryExpressionSyntax)condition;

				if (b.Right.IsKind(SyntaxKind.NullLiteralExpression))
				{
					return b.Left;
				}
				else if (b.Left.IsKind(SyntaxKind.NullLiteralExpression))
				{
					return b.Right;
				}
				else
				{
					return null;
				}
			}
		}

		public static bool IsNonEmptyStringArgumentCheck(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
		{
			var arguments = invoke.ArgumentList.Arguments;

			if (arguments.Count == 0)
			{
				identifierLocation = null;
				return false;
			}

			var expression = arguments[0].Expression;

			if (!expression.IsKind(SyntaxKind.LogicalNotExpression))
			{
				identifierLocation = null;
				return false;
			}

			expression = ((PrefixUnaryExpressionSyntax)expression).Operand;

			if (!expression.IsKind(SyntaxKind.InvocationExpression))
			{
				identifierLocation = null;
				return false;
			}

			var checkInvoke = (InvocationExpressionSyntax)expression;
			var checkArguments = checkInvoke.ArgumentList.Arguments;

			if (ExpressionHelper.GetMethodName(checkInvoke).Identifier.Text != nameof(string.IsNullOrEmpty) ||
				checkArguments.Count != 1)
			{
				identifierLocation = null;
				return false;
			}

			var checkMethodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(checkInvoke, cancellationToken).Symbol;

			if (checkMethodSymbol == null || checkMethodSymbol.ContainingType.SpecialType != SpecialType.System_String)
			{
				identifierLocation = null;
				return false;
			}

			expression = checkArguments[0].Expression;
			var paramSymbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;

			if (paramSymbol == null || paramSymbol.Kind != SymbolKind.Parameter)
			{
				identifierLocation = null;
				return false;
			}

			identifierLocation = expression.GetLocation();
			return true;
		}

		public static bool AccessesParameter(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
		{
			var locator = new ParameterLocator(semanticModel, cancellationToken);
			invoke.ArgumentList.Accept(locator);
			identifierLocation = locator.ParameterLocation;
			return identifierLocation != null;
		}

		public static bool InvokesContractForAll(SemanticModel semanticModel, InvocationExpressionSyntax invoke, CancellationToken cancellationToken)
		{
			var visitor = new ContractForAllVisitor(semanticModel, cancellationToken);
			invoke.ArgumentList.Accept(visitor);
			return visitor.EncounteredContractForAll;
		}

		public static StatementSyntax ConvertGenericRequires(SemanticModel semanticModel, InvocationExpressionSyntax invoke, Location typeLocation, CancellationToken cancellationToken)
		{
			var exceptionType = (TypeSyntax)invoke.FindNode(typeLocation.SourceSpan)
				.WithAdditionalAnnotations(Simplifier.Annotation);

			var arguments = invoke.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression);

			if (arguments.Count > 1)
			{
				arguments = arguments.RemoveAt(0);
			}
			else if (IsNullArgumentCheck(semanticModel, invoke, out var identifierLocation, cancellationToken))
			{
				return ConvertRequiresNotNull(invoke, identifierLocation);
			}
			else if (IsNonEmptyStringArgumentCheck(semanticModel, invoke, out identifierLocation, cancellationToken))
			{
				return ConvertRequires(invoke, identifierLocation, NotNullOrEmptyMessage);
			}
			else
			{
				arguments = default;
			}

			return CreateGuardClause(
				condition,
				exceptionType,
				SyntaxFactory.ArgumentList(arguments));
		}

		public static StatementSyntax ConvertRequiresNotNull(InvocationExpressionSyntax invoke, Location identifierLocation)
		{
			var arguments = invoke.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression);

			ArgumentListSyntax argumentList;

			if (arguments.Count > 1)
			{
				// If an explicit parameter name was provided to Requires(), then keep it.
				argumentList = SyntaxFactory.ArgumentList(arguments.RemoveAt(0));
			}
			else
			{
				var paramSyntax = (ExpressionSyntax)invoke.FindNode(identifierLocation.SourceSpan);
				argumentList = SyntaxFactory.ArgumentList(
					SyntaxFactory.SeparatedList(new[]
					{
						SyntaxFactory.Argument(ExpressionSyntaxFactory.CreateNameof(paramSyntax)),
					}));
			}

			return CreateGuardClause(
				condition,
				ArgumentNullExceptionTypeName,
				argumentList);
		}

		public static StatementSyntax ConvertRequires(InvocationExpressionSyntax invoke, Location identifierLocation, string defaultMessage)
		{
			var arguments = invoke.ArgumentList.Arguments;
			var paramSyntax = (ExpressionSyntax)invoke.FindNode(identifierLocation.SourceSpan, getInnermostNodeForTie: true);

			return CreateArgumentExceptionGuardClause(
				ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression),
				ExpressionSyntaxFactory.CreateNameof(paramSyntax),
				arguments.Count > 1
					? arguments[1].Expression
					: ExpressionSyntaxFactory.CreateLiteral(defaultMessage));
		}

		public static bool IsCodeContractsSuppression(SemanticModel semanticModel, AttributeSyntax attribute)
		{
			var attributeArguments = attribute.ArgumentList?.Arguments;

			if (attributeArguments == null || attributeArguments.Value.Count == 0)
			{
				return false;
			}

			var firstArgument = attributeArguments.Value[0];
			if (!firstArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
			{
				return false;
			}

			var literal = semanticModel.GetConstantValue(firstArgument.Expression);
			return literal.HasValue && literal.Value is string literalValue && literalValue == "Microsoft.Contracts";
		}

		public static SyntaxNode WithElasticTriviaFrom(SyntaxNode target, SyntaxNode source)
		{
			return target
				.WithLeadingTrivia(source.GetLeadingTrivia().Insert(0, SyntaxFactory.ElasticMarker))
				.WithTrailingTrivia(source.GetTrailingTrivia().Add(SyntaxFactory.ElasticMarker));
		}

		static StatementSyntax CreateArgumentExceptionGuardClause(ExpressionSyntax condition, ExpressionSyntax parameterName, ExpressionSyntax message)
		{
			return CreateGuardClause(
				condition,
				ArgumentExceptionTypeName,
				SyntaxFactory.ArgumentList(
					SyntaxFactory.SeparatedList(new[]
					{
						SyntaxFactory.Argument(message),
						SyntaxFactory.Argument(parameterName),
					})));
		}

		static StatementSyntax CreateGuardClause(ExpressionSyntax condition, TypeSyntax exceptionType, ArgumentListSyntax argumentList)
		{
			return SyntaxFactory.IfStatement(
				condition,
				SyntaxFactory.Block(
					SyntaxFactory.ThrowStatement(
						SyntaxFactory.ObjectCreationExpression(
							exceptionType,
							argumentList,
							null))))
				.WithAdditionalAnnotations(Formatter.Annotation);
		}

		static readonly TypeSyntax ArgumentNullExceptionTypeName =
			SyntaxFactory.QualifiedName(
				SyntaxFactory.IdentifierName(nameof(System)),
				SyntaxFactory.IdentifierName(nameof(ArgumentNullException)))
			.WithAdditionalAnnotations(Simplifier.Annotation);

		static readonly TypeSyntax ArgumentExceptionTypeName =
			SyntaxFactory.QualifiedName(
				SyntaxFactory.IdentifierName(nameof(System)),
				SyntaxFactory.IdentifierName(nameof(ArgumentException)))
			.WithAdditionalAnnotations(Simplifier.Annotation);

		sealed class ParameterLocator : CSharpSyntaxWalker
		{
			public ParameterLocator(SemanticModel model, CancellationToken cancellationToken)
			{
				this.model = model;
				this.cancellationToken = cancellationToken;
			}

			public Location ParameterLocation { get; private set; }

			public override void Visit(SyntaxNode node)
			{
				if (ParameterLocation == null)
				{
					base.Visit(node);
				}
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;

				if (symbol != null && symbol.Kind == SymbolKind.Parameter)
				{
					ParameterLocation = node.GetLocation();
				}
			}

			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
			{
			}

			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
			{
			}

			public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
			{
			}

			public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
			{
			}

			readonly SemanticModel model;
			readonly CancellationToken cancellationToken;
		}

		sealed class ContractForAllVisitor : CSharpSyntaxWalker
		{
			public ContractForAllVisitor(SemanticModel model, CancellationToken cancellationToken)
			{
				this.model = model;
				this.cancellationToken = cancellationToken;
			}

			public bool EncounteredContractForAll { get; private set; }

			public override void Visit(SyntaxNode node)
			{
				if (!EncounteredContractForAll)
				{
					base.Visit(node);
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;
				if (symbol != null && symbol.Name == "ForAll" && symbol.ContainingType.IsMatch("System.Diagnostics.Contracts.Contract"))
				{
					EncounteredContractForAll = true;
					return;
				}

				base.VisitInvocationExpression(node);
			}

			readonly SemanticModel model;
			readonly CancellationToken cancellationToken;
		}
	}
}
