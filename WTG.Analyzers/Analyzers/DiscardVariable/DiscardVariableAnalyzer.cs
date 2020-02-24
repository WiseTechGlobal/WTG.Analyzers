using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class DiscardVariableAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.VariableCouldBeConfusedWithDiscardRule);

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
				SyntaxKind.Argument,
				SyntaxKind.ForEachStatement,
				SyntaxKind.Parameter,
				SyntaxKind.VariableDeclarator);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			switch (context.Node.Kind())
			{
				case SyntaxKind.Argument:
					Analyze(context, (ArgumentSyntax)context.Node);
					return;

				case SyntaxKind.ForEachStatement:
					Analyze(context, (ForEachStatementSyntax)context.Node);
					return;

				case SyntaxKind.Parameter:
					Analyze(context, (ParameterSyntax)context.Node);
					return;

				case SyntaxKind.VariableDeclarator:
					Analyze(context, (VariableDeclaratorSyntax)context.Node);
					return;
			}
		}

		static void Analyze(SyntaxNodeAnalysisContext context, ArgumentSyntax node)
		{
			if (node.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword) &&
				node.Expression.IsKind(SyntaxKind.DeclarationExpression))
			{
				var declaration = (DeclarationExpressionSyntax)node.Expression;

				if (declaration.Designation.IsKind(SyntaxKind.DiscardDesignation))
				{
					var identifier = ((DiscardDesignationSyntax)declaration.Designation).UnderscoreToken;
					context.ReportDiagnostic(Rules.CreateVariableCouldBeConfusedWithDiscardDiagnostic(identifier.GetLocation()));
				}
			}
		}

		static void Analyze(SyntaxNodeAnalysisContext context, ForEachStatementSyntax node)
		{
			var identifier = node.Identifier;

			if (identifier.Text == "_")
			{
				context.ReportDiagnostic(Rules.CreateVariableCouldBeConfusedWithDiscardDiagnostic(identifier.GetLocation()));
			}
		}

		static void Analyze(SyntaxNodeAnalysisContext context, ParameterSyntax node)
		{
			var identifier = node.Identifier;

			if (identifier.Text == "_")
			{
				context.ReportDiagnostic(Rules.CreateVariableCouldBeConfusedWithDiscardDiagnostic(identifier.GetLocation()));
			}
		}

		static void Analyze(SyntaxNodeAnalysisContext context, VariableDeclaratorSyntax node)
		{
			var identifier = node.Identifier;

			if (identifier.Text == "_")
			{
				context.ReportDiagnostic(Rules.CreateVariableCouldBeConfusedWithDiscardDiagnostic(identifier.GetLocation()));
			}
		}
	}
}
