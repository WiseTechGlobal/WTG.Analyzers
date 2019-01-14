using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeContractsCodeFixProvider))]
	[Shared]
	public sealed class CodeContractsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseCodeContractsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			if (diagnostic.Properties.TryGetValue(CodeContractsAnalyzer.PropertyProposedFix, out var proposedFix))
			{
				switch (proposedFix)
				{
					case CodeContractsAnalyzer.FixDelete:
						context.RegisterCodeFix(CreateDeleteAction(context.Document, diagnostic), diagnostic);
						break;

					case CodeContractsAnalyzer.FixRequires:
						return RegisterCodeFixesForRequiresAsync(context, diagnostic);
				}
			}

			return Task.FromResult<object>(null);
		}

		static async Task RegisterCodeFixesForRequiresAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			var cancellationToken = context.CancellationToken;
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = GetNode(root, diagnostic);

			if (!node.IsKind(SyntaxKind.ExpressionStatement))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)((ExpressionStatementSyntax)node).Expression;

			if (IsGenericMethod(invoke, out var supplementalLocation))
			{
				context.RegisterCodeFix(
					CreateReplaceWithIfAction(c => FixGenericRequires(document, diagnostic, supplementalLocation, c)),
					diagnostic);
			}
			else
			{
				var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

				if (IsInPrivateMember(semanticModel, invoke, context.CancellationToken))
				{
					context.RegisterCodeFix(CreateDeleteAction(context.Document, diagnostic), diagnostic);
				}
				else if (IsNullArgumentCheck(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequiresNotNull(document, diagnostic, supplementalLocation, c)),
						diagnostic);
				}
				else if (IsNonEmptyStringArgumentCheck(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequires(document, diagnostic, supplementalLocation, "Value cannot be null or empty.", c)),
						diagnostic);
				}
				else if (AccessesParameter(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequires(document, diagnostic, supplementalLocation, "Invalid Argument.", c)),
						diagnostic);
				}
			}
		}

		static async Task<Document> FixByDelete(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			return document.WithSyntaxRoot(
				root.RemoveNode(
					GetNode(root, diagnostic),
					SyntaxRemoveOptions.AddElasticMarker | SyntaxRemoveOptions.KeepExteriorTrivia));
		}

		static async Task<Document> FixGenericRequires(Document document, Diagnostic diagnostic, Location typeLocation, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invoke = (InvocationExpressionSyntax)statementNode.Expression;
			var exceptionType = (TypeSyntax)invoke.FindNode(typeLocation.SourceSpan);
			var arguments = invoke.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression);

			var replacement = CreateGuardClause(
				condition,
				exceptionType,
				arguments.Count > 1 ? SyntaxFactory.ArgumentList(arguments.RemoveAt(0)) : SyntaxFactory.ArgumentList());

			return document.WithSyntaxRoot(
				root.ReplaceNode(statementNode, replacement));
		}

		static async Task<Document> FixRequiresNotNull(Document document, Diagnostic diagnostic, Location identifierLocation, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var arguments = invokeNode.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression);

			ArgumentListSyntax argumentList;

			if (arguments.Count > 1)
			{
				// If an explicit parameter name was provided to Requires(), then keep it.
				argumentList = SyntaxFactory.ArgumentList(arguments.RemoveAt(0));
			}
			else
			{
				var paramSyntax = (ExpressionSyntax)invokeNode.FindNode(identifierLocation.SourceSpan);
				argumentList = SyntaxFactory.ArgumentList(
					SyntaxFactory.SeparatedList(new[]
					{
						SyntaxFactory.Argument(ExpressionSyntaxFactory.CreateNameof(paramSyntax))
					}));
			}

			var replacement = CreateGuardClause(
				condition,
				ArgumentNullExceptionTypeName,
				argumentList);

			return document.WithSyntaxRoot(
				root.ReplaceNode(statementNode, replacement));
		}

		static async Task<Document> FixRequires(Document document, Diagnostic diagnostic, Location identifierLocation, string defaultMessage, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var arguments = invokeNode.ArgumentList.Arguments;
			var paramSyntax = (ExpressionSyntax)invokeNode.FindNode(identifierLocation.SourceSpan, getInnermostNodeForTie: true);

			var replacement = CreateArgumentExceptionGuardClause(
				ExpressionSyntaxFactory.InvertBoolExpression(arguments[0].Expression),
				arguments.Count > 1
					? arguments[1].Expression
					: ExpressionSyntaxFactory.CreateLiteral(defaultMessage),
				ExpressionSyntaxFactory.CreateNameof(paramSyntax));

			return document.WithSyntaxRoot(
				root.ReplaceNode(statementNode, replacement));
		}

		static bool IsGenericMethod(InvocationExpressionSyntax invoke, out Location typeLocation)
		{
			var access = (MemberAccessExpressionSyntax)invoke.Expression;

			if (access.Name.IsKind(SyntaxKind.GenericName))
			{
				var name = (GenericNameSyntax)access.Name;
				var typeArgs = name.TypeArgumentList.Arguments;

				if (typeArgs.Count > 0)
				{
					typeLocation = typeArgs[0].GetLocation();
					return true;
				}
			}

			typeLocation = null;
			return false;
		}

		static bool IsInPrivateMember(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
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

					case SyntaxKind.AddAccessorDeclaration:
					case SyntaxKind.RemoveAccessorDeclaration:
						var eventAccessorDecl = (AccessorDeclarationSyntax)node;
						var eventDecl = (EventDeclarationSyntax)node.Parent;
						return eventDecl?.ExplicitInterfaceSpecifier != null && IsPrivate(semanticModel.GetDeclaredSymbol(eventAccessorDecl, cancellationToken));

					case SyntaxKind.GetAccessorDeclaration:
					case SyntaxKind.SetAccessorDeclaration:
						var propertyAccessorDecl = (AccessorDeclarationSyntax)node;
						var propertyDecl = (PropertyDeclarationSyntax)node.Parent;
						return propertyDecl?.ExplicitInterfaceSpecifier != null && IsPrivate(semanticModel.GetDeclaredSymbol(propertyAccessorDecl, cancellationToken));

					case SyntaxKind.NamespaceDeclaration:
					case SyntaxKind.CompilationUnit:
					case SyntaxKind.ClassDeclaration:
					case SyntaxKind.StructDeclaration:
					case SyntaxKind.UnknownAccessorDeclaration:
						return false;
				}

				node = node.Parent;
			}

			return false;

			bool IsPrivate(ISymbol symbol) => symbol != null && symbol.DeclaredAccessibility == Accessibility.Private;
		}

		static bool IsNullArgumentCheck(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
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

			ExpressionSyntax GetComparand(ExpressionSyntax condition)
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

		static bool IsNonEmptyStringArgumentCheck(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
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

		static bool AccessesParameter(SemanticModel semanticModel, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
		{
			var locator = new ParameterLocator(semanticModel, cancellationToken);
			invoke.ArgumentList.Accept(locator);
			identifierLocation = locator.ParameterLocation;
			return identifierLocation != null;
		}

		static CodeAction CreateDeleteAction(Document document, Diagnostic diagnostic)
		{
			return CodeAction.Create(
				"Delete.",
				c => FixByDelete(document, diagnostic, c),
				equivalenceKey: "Delete");
		}

		static CodeAction CreateReplaceWithIfAction(Func<CancellationToken, Task<Document>> createChangedDocument)
		{
			return CodeAction.Create(
				"Replace with 'if' check.",
				createChangedDocument,
				equivalenceKey: "ReplaceWithIf");
		}

		static SyntaxNode GetNode(SyntaxNode root, Diagnostic diagnostic)
		{
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			return root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
		}

		static StatementSyntax CreateArgumentExceptionGuardClause(ExpressionSyntax condition, ExpressionSyntax message, ExpressionSyntax parameterName)
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
	}
}
