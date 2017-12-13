using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class ArrayAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rules.PreferArrayEmptyOverNewArrayConstructionRule);

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
				SyntaxKind.ArrayCreationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var syntax = (ArrayCreationExpressionSyntax)context.Node;

			if (syntax.FirstAncestorOrSelf<AttributeArgumentSyntax>() != null)
			{
				// Ignore empty arrays in attributes. Array.Empty<T>() cannot be used here.
				return;
			}

			var ranks = syntax.Type.RankSpecifiers;
			if (ranks.Count != 1)
			{
				// Ignore jagged arrays.
				return;
			}

			var sizes = ranks[0].Sizes;

			if (sizes.Count != 1)
			{
				// Ignore multi-dimensional arrays.
				return;
			}

			var size = sizes[0];
			if (size.Kind() != SyntaxKind.NumericLiteralExpression)
			{
				// Ignore dynamically sized arrays.
				return;
			}

			var literal = (LiteralExpressionSyntax)size;
			if (literal.Token.Text != "0")
			{
				// Ignore statically sized arrays that are not empty;
				return;
			}

			context.ReportDiagnostic(Rules.CreatePreferArrayEmptyOverNewArrayConstructionDiagnostic(context.Node.GetLocation()));
		}
	}
}
