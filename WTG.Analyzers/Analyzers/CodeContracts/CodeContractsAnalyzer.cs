using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;
using SDC = System.Diagnostics.Contracts;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class CodeContractsAnalyzer : DiagnosticAnalyzer
	{
		public const string PropertyProposedFix = "FIX";

		public const string FixUnavailable = "U";
		public const string FixDelete = "D";
		public const string FixGenericRequires = "RG";
		public const string FixRequiresNonNull = "RN";
		public const string FixRequiresNonEmptyString = "RS";
		public const string FixRequires = "R";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotUseCodeContractsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
			context.RegisterSyntaxNodeAction(AnalyzeInvoke, SyntaxKind.InvocationExpression);
		}

		void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
		{
			var attributeNode = (AttributeSyntax)context.Node;
			var attributeSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(attributeNode, context.CancellationToken).Symbol;

			if (attributeSymbol == null || !attributeSymbol.ContainingNamespace.IsMatch("System.Diagnostics.Contracts"))
			{
				return;
			}

			Location location;

			switch (attributeSymbol.ContainingType.Name)
			{
				case nameof(SDC.ContractAbbreviatorAttribute):
				case nameof(SDC.ContractArgumentValidatorAttribute):
				case nameof(SDC.ContractClassAttribute):
				case nameof(SDC.ContractOptionAttribute):
				case nameof(SDC.ContractPublicPropertyNameAttribute):
				case nameof(SDC.ContractReferenceAssemblyAttribute):
				case nameof(SDC.ContractVerificationAttribute):
				case nameof(SDC.PureAttribute):
				case nameof(SDC.ContractRuntimeIgnoredAttribute):
					location = AttributeUtils.GetLocation(attributeNode);
					break;

				case nameof(SDC.ContractClassForAttribute):
					location = GetAttributedMemberLocation(attributeNode, SyntaxKind.ClassDeclaration);
					break;

				case nameof(SDC.ContractInvariantMethodAttribute):
					location = GetAttributedMemberLocation(attributeNode, SyntaxKind.MethodDeclaration);
					break;

				default:
					return;
			}

			context.ReportDiagnostic(FixDeleteDiagnostic(location));
		}

		void AnalyzeInvoke(SyntaxNodeAnalysisContext context)
		{
			var invoke = (InvocationExpressionSyntax)context.Node;
			var name = ExpressionHelper.GetMethodName(invoke);

			if (name == null)
			{
				return;
			}

			bool isRequires;

			switch (name.Identifier.Text)
			{
				case nameof(SDC.Contract.Assert):
				case nameof(SDC.Contract.Assume):
				case nameof(SDC.Contract.EndContractBlock):
				case nameof(SDC.Contract.Ensures):
				case nameof(SDC.Contract.EnsuresOnThrow):
					isRequires = false;
					break;

				case nameof(SDC.Contract.Requires):
					isRequires = true;
					break;

				default:
					return;
			}

			var symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

			if (symbol == null || !symbol.ContainingType.IsMatch("System.Diagnostics.Contracts.Contract"))
			{
				return;
			}

			if (!invoke.Parent.IsKind(SyntaxKind.ExpressionStatement))
			{
				// All these methods return void, so if the parent is not an ExpressionStatement,
				// then something crazy is happening and we can't provide a meaningful auto-fix.
				context.ReportDiagnostic(FixUnavailableDiagnostic(invoke.GetLocation()));
			}
			else
			{
				var statement = (ExpressionStatementSyntax)invoke.Parent;

				if (!isRequires)
				{
					// Only Contract.Requires needs special handling, all the rest are simply deleted.
					context.ReportDiagnostic(FixDeleteDiagnostic(statement.GetLocation()));
				}
				else if (symbol.IsGenericMethod)
				{
					context.ReportDiagnostic(FixGenericRequiresDiagnostic(statement.GetLocation()));
				}
				else if (IsInPrivateMember(context.SemanticModel, statement, context.CancellationToken))
				{
					context.ReportDiagnostic(FixDeleteDiagnostic(statement.GetLocation()));
				}
				else if (IsNullArgumentCheck(context.SemanticModel, invoke, out var identifierLocation, context.CancellationToken))
				{
					context.ReportDiagnostic(FixRequiresNonNullDiagnostic(statement.GetLocation(), identifierLocation));
				}
				else if (IsNonEmptyStringArgumentCheck(context.SemanticModel, invoke, out identifierLocation, context.CancellationToken))
				{
					context.ReportDiagnostic(FixRequiresNonEmptyStringDiagnostic(statement.GetLocation(), identifierLocation));
				}
				else if (RequiresParameter(context.SemanticModel, invoke, out identifierLocation, context.CancellationToken))
				{
					context.ReportDiagnostic(FixRequiresDiagnostic(statement.GetLocation(), identifierLocation));
				}
				else
				{
					context.ReportDiagnostic(FixUnavailableDiagnostic(statement.GetLocation()));
				}
			}
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

		static bool RequiresParameter(SemanticModel model, InvocationExpressionSyntax invoke, out Location identifierLocation, CancellationToken cancellationToken)
		{
			var locator = new ParameterLocator(model, cancellationToken);
			invoke.ArgumentList.Accept(locator);
			identifierLocation = locator.ParameterLocation;
			return identifierLocation != null;
		}

		static bool IsInPrivateMember(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
		{
			while (node != null)
			{
				switch (node.Kind())
				{
					case SyntaxKind.MethodDeclaration:
						var methodDecl = (MethodDeclarationSyntax)node;
						return methodDecl.ExplicitInterfaceSpecifier == null && IsPrivate(model.GetDeclaredSymbol(methodDecl));

					case SyntaxKind.ConstructorDeclaration:
						return IsPrivate(model.GetDeclaredSymbol((ConstructorDeclarationSyntax)node, cancellationToken));

					case SyntaxKind.DestructorDeclaration:
						return IsPrivate(model.GetDeclaredSymbol((DestructorDeclarationSyntax)node, cancellationToken));

					case SyntaxKind.AddAccessorDeclaration:
					case SyntaxKind.RemoveAccessorDeclaration:
						var eventAccessorDecl = (AccessorDeclarationSyntax)node;
						var eventDecl = (EventDeclarationSyntax)node.Parent;
						return eventDecl?.ExplicitInterfaceSpecifier != null && IsPrivate(model.GetDeclaredSymbol(eventAccessorDecl, cancellationToken));

					case SyntaxKind.GetAccessorDeclaration:
					case SyntaxKind.SetAccessorDeclaration:
						var propertyAccessorDecl = (AccessorDeclarationSyntax)node;
						var propertyDecl = (PropertyDeclarationSyntax)node.Parent;
						return propertyDecl?.ExplicitInterfaceSpecifier != null && IsPrivate(model.GetDeclaredSymbol(propertyAccessorDecl, cancellationToken));

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

		static Location GetAttributedMemberLocation(AttributeSyntax attribute, SyntaxKind expectedKind)
		{
			var attributeList = attribute.Parent;

			if (attributeList != null)
			{
				var owner = attributeList.Parent;

				if (owner.IsKind(expectedKind))
				{
					return owner.GetLocation();
				}
			}

			return AttributeUtils.GetLocation(attribute);
		}

		static Diagnostic FixUnavailableDiagnostic(Location location)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, FixUnavailableProperties);

		static Diagnostic FixDeleteDiagnostic(Location location)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, FixDeleteProperties);

		static Diagnostic FixGenericRequiresDiagnostic(Location location)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, FixGenericRequiresProperties);

		static Diagnostic FixRequiresNonNullDiagnostic(Location location, Location identifierLocation)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, new[] { identifierLocation }, FixRequiresNonNullProperties);

		static Diagnostic FixRequiresNonEmptyStringDiagnostic(Location location, Location identifierLocation)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, new[] { identifierLocation }, FixRequiresNonEmptyStringProperties);

		static Diagnostic FixRequiresDiagnostic(Location location, Location identifierLocation)
			=> Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, new[] { identifierLocation }, FixRequiresProperties);

		static readonly ImmutableDictionary<string, string> FixUnavailableProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixUnavailable);
		static readonly ImmutableDictionary<string, string> FixDeleteProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixDelete);
		static readonly ImmutableDictionary<string, string> FixGenericRequiresProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixGenericRequires);
		static readonly ImmutableDictionary<string, string> FixRequiresNonNullProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixRequiresNonNull);
		static readonly ImmutableDictionary<string, string> FixRequiresNonEmptyStringProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixRequiresNonEmptyString);
		static readonly ImmutableDictionary<string, string> FixRequiresProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixRequires);

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
