using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class ConditionalOperandAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.ConditionalOperatorsShouldNotHaveMultilineValues_WhenFalseRule,
			Rules.ConditionalOperatorsShouldNotHaveMultilineValues_WhenTrueRule);

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
				SyntaxKind.ConditionalExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (ConditionalExpressionSyntax)context.Node;

			CheckOperand(
				ref context,
				Rules.ConditionalOperatorsShouldNotHaveMultilineValues_WhenTrueRule,
				node.QuestionToken,
				node.WhenTrue);

			CheckOperand(
				ref context,
				Rules.ConditionalOperatorsShouldNotHaveMultilineValues_WhenFalseRule,
				node.ColonToken,
				node.WhenFalse);
		}

		static void CheckOperand(ref SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule, SyntaxToken leadingToken, ExpressionSyntax expression)
		{
			if (expression == null)
			{
				return;
			}

			var l1 = leadingToken.GetLocation();
			var l2 = expression.GetLocation();
			NRT.Assert(l1.SourceTree != null, "'leadingToken' should have been taken from a complete SyntaxTree, so the SourceTree from it's location should not be null.");
			NRT.Assert(l2.SourceTree != null, "'expression' should have been taken from a complete SyntaxTree, so the SourceTree from it's location should not be null.");

			if (l1.GetMappedLineSpan().StartLinePosition.Line != l2.GetMappedLineSpan().EndLinePosition.Line)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						rule,
						Location.Create(l1.SourceTree, TextSpan.FromBounds(l1.SourceSpan.Start, l2.SourceSpan.End))));
			}
		}
	}
}
