using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InterpolatedStringAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Rules.InterpolatedStringMustBePurposefulRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();

			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(c => Analyze(c, cache), SyntaxKind.InterpolatedStringExpression);
		}

		public static void Analyze (SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (InterpolatedStringExpressionSyntax)context.Node;

			if (node.Contents.Count != 1)
			{
				return;
			}

			if (node.Contents.First().IsKind(SyntaxKind.Interpolation))
			{
				var interpolation = (InterpolationSyntax)node.Contents[0];

				if (interpolation.AlignmentClause != null)
				{
					return;
				}
			}

			var info = context.SemanticModel.GetTypeInfo(node);

			if (info.ConvertedType != null && info.ConvertedType.SpecialType != SpecialType.System_String)
			{
				return;
			}

			context.ReportDiagnostic(Rules.CreateInterpolatedStringMustBePurposefulDiagnostic(
				node.GetLocation()));
		}
	}
}
