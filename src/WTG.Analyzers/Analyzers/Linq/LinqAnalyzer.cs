using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class LinqAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.PreferDirectMemberAccessOverLinq_UseIndexerRule,
			Rules.PreferDirectMemberAccessOverLinq_UsePropertyRule,
			Rules.PreferDirectMemberAccessOverLinqInAnExpression_UseIndexerRule,
			Rules.PreferDirectMemberAccessOverLinqInAnExpression_UsePropertyRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)context.Node;
			var methodName = ExpressionHelper.GetMethodName(invoke);

			if (methodName == null)
			{
				return;
			}

			var linqMethod = LinqMethod.Find(methodName.Identifier.Text);

			if (linqMethod == null)
			{
				return;
			}

			var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke).Symbol;

			if (methodSymbol == null || !linqMethod.IsMatch(methodSymbol))
			{
				return;
			}

			var sourceExpression = methodSymbol.IsStatic
				? invoke.ArgumentList.Arguments[0].Expression
				: LinqUtils.GetInstanceExpression(invoke);

			if (sourceExpression == null)
			{
				return;
			}

			var sourceType = context.SemanticModel.GetTypeInfo(sourceExpression).Type;

			if (sourceType == null)
			{
				return;
			}

			var resolution = linqMethod.GetResolution(sourceType.OriginalDefinition);

			if (resolution != null)
			{
				var isInAnExpression = ExpressionHelper.IsContainedInExpressionTree(context.SemanticModel, invoke, context.CancellationToken);
				context.ReportDiagnostic(resolution.CreateDiagnostic(invoke, sourceType, isInAnExpression));
			}
		}
	}
}
