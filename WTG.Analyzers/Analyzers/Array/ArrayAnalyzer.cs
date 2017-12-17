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
			if (ranks.Count == 0)
			{
				// Incomplete code?
				return;
			}

			var rank = ranks[0];
			if (rank.Sizes.Count != 1)
			{
				// Ignore multi-dimensional top-level arrays.
				return;
			}

			foreach (var size in rank.Sizes)
			{
				switch (size.Kind())
				{
					case SyntaxKind.OmittedArraySizeExpression:
						if (syntax.Initializer == null)
						{
							// If there's an initializer, we check with the initializer expression count below.
							// If there's no initializer, this is a compiler error anyhow, so don't bother the
							// developer with an analyzer result.
							return;
						}
						break;

					case SyntaxKind.CharacterLiteralExpression:
					case SyntaxKind.NumericLiteralExpression:
					case SyntaxKind.CastExpression:
					case SyntaxKind.UnaryMinusExpression:
					case SyntaxKind.UnaryPlusExpression:
						var constant = context.SemanticModel.GetConstantValue(size, context.CancellationToken);
						if (constant.HasValue && !IsZeroLiteral(constant.Value))
						{
							return;
						}
						break;

					default:
						return;
				}
			}

			if (syntax.Initializer?.Expressions.Count > 0)
			{
				return;
			}

			context.ReportDiagnostic(Rules.CreatePreferArrayEmptyOverNewArrayConstructionDiagnostic(context.Node.GetLocation()));
		}

		static bool IsZeroLiteral(object value)
		{
			switch (value)
			{
				case int s:
					return s == 0;

				case uint s:
					return s == 0;

				case long s:
					return s == 0;

				case ulong s:
					return s == 0;

				case byte s:
					return s == 0;

				case short s:
					return s == 0;

				case ushort s:
					return s == 0;

				case sbyte s:
					return s == 0;

				case char s:
					return s == 0;

				default:
					return false;
			}
		}
	}
}
