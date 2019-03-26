using System.Collections.Immutable;
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

		public const string FixDelete = "D";
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

			if (attributeSymbol == null)
			{
				return;
			}

			Location location;

			if (attributeSymbol.ContainingType.IsMatch("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute") && CodeContractsHelper.IsCodeContractsSuppression(context.SemanticModel, attributeNode))
			{
				location = AttributeUtils.GetLocation(attributeNode);
			}
			else if (attributeSymbol.ContainingNamespace.IsMatch("System.Diagnostics.Contracts"))
			{
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
			}
			else
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, FixDeleteProperties));
		}

		void AnalyzeInvoke(SyntaxNodeAnalysisContext context)
		{
			var invoke = (InvocationExpressionSyntax)context.Node;
			var name = ExpressionHelper.GetMethodName(invoke);

			if (name == null)
			{
				return;
			}

			ImmutableDictionary<string, string> properties;

			switch (name.Identifier.Text)
			{
				case nameof(SDC.Contract.Assert):
				case nameof(SDC.Contract.Assume):
				case nameof(SDC.Contract.EndContractBlock):
				case nameof(SDC.Contract.Ensures):
				case nameof(SDC.Contract.EnsuresOnThrow):
					properties = FixDeleteProperties;
					break;

				case nameof(SDC.Contract.Requires):
					// Only Contract.Requires needs special handling, all the rest are simply deleted.
					properties = FixRequiresProperties;
					break;

				default:
					return;
			}

			var symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

			if (symbol == null || !symbol.ContainingType.IsMatch("System.Diagnostics.Contracts.Contract"))
			{
				return;
			}

			var location = invoke.Parent.IsKind(SyntaxKind.ExpressionStatement)
				? ((ExpressionStatementSyntax)invoke.Parent).GetLocation()
				: invoke.GetLocation();

			context.ReportDiagnostic(Diagnostic.Create(Rules.DoNotUseCodeContractsRule, location, properties));
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

		static readonly ImmutableDictionary<string, string> FixDeleteProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixDelete);
		static readonly ImmutableDictionary<string, string> FixRequiresProperties = ImmutableDictionary<string, string>.Empty.Add(PropertyProposedFix, FixRequires);
	}
}
