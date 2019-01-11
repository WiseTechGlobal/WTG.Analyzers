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

			context.ReportDiagnostic(Diagnostic.Create(
				Rules.DoNotUseCodeContractsRule,
				location,
				FixDeleteProperties));
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

			var symbol = GetMethodSymbol(context.SemanticModel, invoke, context.CancellationToken);

			if (symbol == null || !IsContractMethod(symbol))
			{
				return;
			}

			if (!(invoke.Parent is ExpressionStatementSyntax statement))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Rules.DoNotUseCodeContractsRule,
					invoke.GetLocation(),
					FixUnavailable));

				return;
			}

			ImmutableDictionary<string, string> properties;

			if (!isRequires)
			{
				properties = FixDeleteProperties;
			}
			else if (symbol.IsGenericMethod)
			{
				properties = FixGenericRequiresProperties;
			}
			else if (IsInPrivateMember(context.SemanticModel, statement, context.CancellationToken))
			{
				properties = FixDeleteProperties;
			}
			else
			{
				properties = FixUnavailableProperties;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				Rules.DoNotUseCodeContractsRule,
				statement.GetLocation(),
				properties));
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

		static IMethodSymbol GetMethodSymbol(SemanticModel model, InvocationExpressionSyntax invoke, CancellationToken cancellationToken)
			=> (IMethodSymbol)model.GetSymbolInfo(invoke, cancellationToken).Symbol;

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

		static bool IsContractMethod(IMethodSymbol methodSymbol) => methodSymbol.ContainingType.IsMatch("System.Diagnostics.Contracts.Contract");

		static readonly ImmutableDictionary<string, string> FixUnavailableProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixUnavailable);
		static readonly ImmutableDictionary<string, string> FixDeleteProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixDelete);
		static readonly ImmutableDictionary<string, string> FixGenericRequiresProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixGenericRequires);
	}
}
