using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class NestedConditionalAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontNestConditionalOperatorsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.ConditionalExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var condition = (ConditionalExpressionSyntax)context.Node;

			if (IsWithinConditionalOperator(condition))
			{
				context.ReportDiagnostic(Rules.CreateDontNestConditionalOperatorsDiagnostic(condition.GetLocation()));
			}
		}

		static bool IsWithinConditionalOperator(ExpressionSyntax expression)
		{
			for (var tmp = expression.Parent; tmp != null; tmp = tmp.Parent)
			{
				switch (tmp.Kind())
				{
					case SyntaxKind.CompilationUnit:
					case SyntaxKind.NamespaceDeclaration:
					case SyntaxKind.ClassDeclaration:
					case SyntaxKind.StructDeclaration:
					case SyntaxKind.MethodDeclaration:
					case SyntaxKind.PropertyDeclaration:
					case SyntaxKind.AccessorList:
						return false;

					case SyntaxKind.ConditionalExpression:
						return true;
				}
			}

			return false;
		}
	}
}
