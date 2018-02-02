using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class VisibilityAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotUseThePrivateKeywordRule,
			Rules.DoNotUseTheInternalKeywordForTopLevelTypesRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();
			context.RegisterSyntaxNodeAction(c => Analyze(c, cache), ModifierExtractionVisitor.SupportedSyntaxKinds);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var list = ModifierExtractionVisitor.Instance.Visit(context.Node);
			var hasProtected = false;
			Location privateLocation = null;

			foreach (var modifier in list)
			{
				var kind = modifier.Kind();

				switch (kind)
				{
					case SyntaxKind.PrivateKeyword:
						privateLocation = modifier.GetLocation();
						break;

					case SyntaxKind.ProtectedKeyword:
						hasProtected = true;
						break;

					case SyntaxKind.InternalKeyword:
						if (IsTopLevel(context.Node))
						{
							context.ReportDiagnostic(Rules.CreateDoNotUseTheInternalKeywordForTopLevelTypesDiagnostic(modifier.GetLocation()));
						}
						break;
				}
			}

			if (privateLocation != null && !hasProtected)
			{
				context.ReportDiagnostic(Rules.CreateDoNotUseThePrivateKeywordDiagnostic(privateLocation));
			}
		}

		static bool IsTopLevel(SyntaxNode node)
		{
			var parentKind = node.Parent?.Kind() ?? SyntaxKind.None;

			switch (parentKind)
			{
				case SyntaxKind.NamespaceDeclaration:
				case SyntaxKind.CompilationUnit:
					return true;

				default:
					return false;
			}
		}
	}
}
