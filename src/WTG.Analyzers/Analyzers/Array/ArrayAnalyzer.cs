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
			if (!HasArrayEmpty(context.Compilation))
			{
				return;
			}

			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => AnalyzeCreation(c, cache),
				SyntaxKind.ArrayCreationExpression);

			context.RegisterSyntaxNodeAction(
				c => AnalyzeInitializer(c, cache),
				SyntaxKind.ArrayInitializerExpression);
		}

		static void AnalyzeCreation(SyntaxNodeAnalysisContext context, FileDetailCache cache)
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

					default:
						if (!context.SemanticModel.IsConstantZero(size, context.CancellationToken))
						{
							return;
						}
						break;
				}
			}

			if (syntax.Initializer?.Expressions.Count > 0)
			{
				return;
			}

			context.ReportDiagnostic(Rules.CreatePreferArrayEmptyOverNewArrayConstructionDiagnostic(context.Node.GetLocation()));
		}

		static void AnalyzeInitializer(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var syntax = (InitializerExpressionSyntax)context.Node;

			if (syntax.Expressions.Count > 0)
			{
				return;
			}

			if (!syntax.Parent.IsKind(SyntaxKind.EqualsValueClause))
			{
				return;
			}

			var equalsValue = syntax.Parent;
			if (!equalsValue.Parent.IsKind(SyntaxKind.VariableDeclarator))
			{
				return;
			}

			var declarator = equalsValue.Parent;
			if (!declarator.Parent.IsKind(SyntaxKind.VariableDeclaration))
			{
				return;
			}

			var declaration = (VariableDeclarationSyntax)declarator.Parent;
			if (!declaration.Type.IsKind(SyntaxKind.ArrayType))
			{
				return;
			}

			context.ReportDiagnostic(Rules.CreatePreferArrayEmptyOverNewArrayConstructionDiagnostic(context.Node.GetLocation()));
		}

		static bool HasArrayEmpty(Compilation compilation)
		{
			var arraySymbol = compilation.GetTypeByMetadataName("System.Array");

			if (arraySymbol != null)
			{
				foreach (var symbol in arraySymbol.GetMembers(nameof(System.Array.Empty)))
				{
					if (symbol.Kind == SymbolKind.Method)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
