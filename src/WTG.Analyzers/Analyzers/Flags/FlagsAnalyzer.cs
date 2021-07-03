using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class FlagsAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Rules.FlagEnumsShouldSpecifyExplicitValuesRule);

		public override void Initialize(AnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.EnumDeclaration);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var decl = (EnumDeclarationSyntax)context.Node;
			var checkedIsFlag = false;

			foreach (var member in decl.Members)
			{
				if (member.EqualsValue == null)
				{
					if (!checkedIsFlag)
					{
						if (!IsFlagsEnum(context.SemanticModel, decl, context.CancellationToken))
						{
							return;
						}

						checkedIsFlag = true;
					}

					context.ReportDiagnostic(Diagnostic.Create(
						Rules.FlagEnumsShouldSpecifyExplicitValuesRule,
						member.Identifier.GetLocation()));
				}
			}
		}

		static bool IsFlagsEnum(SemanticModel model, EnumDeclarationSyntax decl, CancellationToken cancellationToken)
		{
			bool isFlagsEnum = false;

			foreach (var attributeList in decl.AttributeLists)
			{
				foreach (var attribute in attributeList.Attributes)
				{
					var symbol = (IMethodSymbol)model.GetSymbolInfo(attribute, cancellationToken).Symbol;

					if (symbol != null && symbol.ContainingType.IsMatch("System.FlagsAttribute"))
					{
						isFlagsEnum = true;
					}
				}
			}

			return isFlagsEnum;
		}
	}
}
