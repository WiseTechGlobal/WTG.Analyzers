using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class BooleanLiteralArgumentAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UseNamedArgumentsWhenPassingBooleanLiteralsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp4))
			{
				return;
			}

			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.TrueLiteralExpression,
				SyntaxKind.FalseLiteralExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var expression = (LiteralExpressionSyntax)context.Node;
			if (!expression.Parent.IsKind(SyntaxKind.Argument))
			{
				return;
			}

			var argument = (ArgumentSyntax)expression.Parent;
			if (argument.NameColon != null)
			{
				// We already are a named parameter.
				return;
			}

			if (!argument.Parent.IsKind(SyntaxKind.ArgumentList))
			{
				return;
			}

			var argumentList = (ArgumentListSyntax)argument.Parent;

			if (argumentList.Arguments.Count < 2)
			{
				// Exclude single-parameter calls, hopefully the API designers designed these methods
				// to be clear and understandable enough at the call-site.
				return;
			}

			var index = argumentList.Arguments.IndexOf(argument);

			if (index + 1 < argumentList.Arguments.Count && !context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp7_2))
			{
				// C# 7.2 introduced the ability to name parameters other than the last one.
				// Before that, we probably shouldn't require the user to name all their other parameters, or to upgrade.
				return;
			}

			if (ExpressionHelper.IsContainedInExpressionTree(context.SemanticModel, expression, context.CancellationToken))
			{
				// Avoid error CS0853: An expression tree may not contain a named argument specification.
				return;
			}

			var argumentSymbol = argumentList.TryFindCorrespondingParameterSymbol(context.SemanticModel, index, context.CancellationToken);

			if (argumentSymbol is null || argumentSymbol.IsParams || argumentSymbol.Type.SpecialType == SpecialType.System_Object || argumentSymbol.OriginalDefinition.Type.TypeKind == TypeKind.TypeParameter)
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.UseNamedArgumentsWhenPassingBooleanLiteralsRule,
					argument.GetLocation()));
		}
	}
}
