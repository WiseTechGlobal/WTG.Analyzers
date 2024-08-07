using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

			var currentNode = context.Node;
			var list = ModifierExtractionVisitor.Instance.Visit(currentNode);
			var privateToken = default(SyntaxToken);

			foreach (var modifier in list)
			{
				var kind = modifier.Kind();

				switch (kind)
				{
					case SyntaxKind.PrivateKeyword:
						privateToken = modifier;
						break;

					case SyntaxKind.ProtectedKeyword:
					case SyntaxKind.PublicKeyword:
					case SyntaxKind.PartialKeyword when (PartialMethodRequiresAccessibilityModifier(currentNode)):
						return;
					case SyntaxKind.InternalKeyword:
						if (IsTopLevel(currentNode))
						{
							context.ReportDiagnostic(Rules.CreateDoNotUseTheInternalKeywordForTopLevelTypesDiagnostic(modifier.GetLocation()));
						}
						return;
				}
			}

			if (privateToken.Kind() == SyntaxKind.PrivateKeyword)
			{
				context.ReportDiagnostic(Rules.CreateDoNotUseThePrivateKeywordDiagnostic(privateToken.GetLocation()));
			}
		}

		static bool PartialMethodRequiresAccessibilityModifier(SyntaxNode node)
		{
			if (!node.IsKind(SyntaxKind.MethodDeclaration))
			{
				return false;
			}

			var methodNode = (MethodDeclarationSyntax)node;
			if (methodNode.ReturnType.IsKind(SyntaxKind.PredefinedType))
			{
				var returnType = (PredefinedTypeSyntax)methodNode.ReturnType;
				if (!returnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
				{
					return true;
				}
			}

			if (methodNode.ParameterList?.Parameters.Any(static p => p.Modifiers.Any(SyntaxKind.OutKeyword)) == true)
			{
				return true;
			}

			return false;
		}

		static bool IsTopLevel(SyntaxNode node)
		{
			var parentKind = node.Parent?.Kind() ?? SyntaxKind.None;

			switch (parentKind)
			{
				case SyntaxKind.NamespaceDeclaration:
				case SyntaxKind.CompilationUnit:
				case FutureSyntaxKinds.FileScopedNamespaceDeclaration:
					return true;

				default:
					return false;
			}
		}
	}
}
