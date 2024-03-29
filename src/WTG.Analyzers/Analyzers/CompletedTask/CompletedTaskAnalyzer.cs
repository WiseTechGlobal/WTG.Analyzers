using System.Collections.Immutable;
using System.Threading.Tasks;
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
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!HasCompletedTask(context.Compilation))
			{
				return;
			}

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
			IMethodSymbol? symbol;

			switch (ExpressionHelper.GetMethodName(invoke)?.Identifier.Text)
			{
				case nameof(Task.Delay):
					var arguments = invoke.ArgumentList.Arguments;

					if (arguments.Count != 1)
					{
						return;
					}

					symbol = (IMethodSymbol?)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

					if (symbol == null ||
						!symbol.IsMatch(WellKnownTypeNames.Task, nameof(Task.Delay)) ||
						!context.SemanticModel.IsConstantZero(arguments[0].Expression, context.CancellationToken))
					{
						return;
					}
					break;

				case nameof(Task.FromResult):
					symbol = (IMethodSymbol?)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

					if (symbol == null || !symbol.IsMatch(WellKnownTypeNames.Task, nameof(Task.FromResult)))
					{
						return;
					}

					var convertedType = context.SemanticModel.GetTypeInfo(invoke, context.CancellationToken).ConvertedType;

					if (convertedType == null || !convertedType.IsMatch(WellKnownTypeNames.Task))
					{
						return;
					}
					break;

				default:
					return;
			}

			context.ReportDiagnostic(Rules.CreatePreferCompletedTaskDiagnostic(invoke.GetLocation()));
		}

		static bool HasCompletedTask(Compilation compilation)
		{
			if (compilation.GetTypeByMetadataName(WellKnownTypeNames.Task) is { } taskType)
			{
				foreach (var symbol in taskType.GetMembers(nameof(Task.CompletedTask)))
				{
					if (symbol.Kind == SymbolKind.Property)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
