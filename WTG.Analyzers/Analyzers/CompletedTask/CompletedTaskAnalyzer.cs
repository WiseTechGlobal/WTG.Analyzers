using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class CompletedTaskAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.PreferCompletedTaskRule);

		public override void Initialize(AnalysisContext context)
		{
			//context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
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
			IMethodSymbol symbol;

			switch (ExpressionHelper.GetMethodName(invoke)?.Identifier.Text)
			{
				case "Delay":
					var arguments = invoke.ArgumentList.Arguments;

					if (arguments.Count != 1)
					{
						return;
					}

					symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

					if (symbol == null ||
						!symbol.IsMatch("System.Threading.Tasks.Task", "Delay") ||
						!context.SemanticModel.IsConstantZero(arguments[0].Expression, context.CancellationToken))
					{
						return;
					}
					break;

				case "FromResult":
					symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

					if (symbol == null || !symbol.IsMatch("System.Threading.Tasks.Task", "FromResult"))
					{
						return;
					}

					var convertedType = context.SemanticModel.GetTypeInfo(invoke, context.CancellationToken).ConvertedType;

					if (convertedType == null || !convertedType.IsMatch("System.Threading.Tasks.Task"))
					{
						return;
					}
					break;

				default:
					return;
			}

			context.ReportDiagnostic(Rules.CreatePreferCompletedTaskDiagnostic(invoke.GetLocation()));
		}
	}
}
