using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DeconstructionAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Rules.DeconstructWithSingleVarRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();

			var cache = new FileDetailCache();
			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.TupleExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			if (context.Node.IsKind(SyntaxKind.TupleExpression))
			{
				var tupleExpression = (TupleExpressionSyntax)context.Node;
				
				if (AllTupleArgumentsAreVarDeclarations(tupleExpression))
				{
					context.ReportDiagnostic(Diagnostic.Create(Rules.DeconstructWithSingleVarRule, context.Node.GetLocation()));
				}
			}
		}

		static bool AllTupleArgumentsAreVarDeclarations(TupleExpressionSyntax tupleExpression)
		{
			if (tupleExpression.Arguments.Count == 0)
			{
				return true;
			}

			foreach (var argument in tupleExpression.Arguments)
			{
				switch (argument.Expression.Kind())
				{
					case SyntaxKind.TupleExpression:
						if (!AllTupleArgumentsAreVarDeclarations(((TupleExpressionSyntax)argument.Expression)))
						{
							return false;
						}
						continue;

					case SyntaxKind.DeclarationExpression:
						var declaration = (DeclarationExpressionSyntax)argument.Expression;
						if (!declaration.Type.IsVar)
						{
							return false;
						}
						break;

					default:
						return false;
				}
			}

			return true;
		}
	}
}
