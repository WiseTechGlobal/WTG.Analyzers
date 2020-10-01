using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class BooleanLiteralAnalyzer : DiagnosticAnalyzer
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
				return;
			}

			var methodInvocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
			if (methodInvocation is null)
			{
				return;
			}

			var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(methodInvocation, context.CancellationToken).Symbol;
			if (methodSymbol is null)
			{
				return;
			}

			var argumentList = (ArgumentListSyntax)argument.Parent;
			var index = FindArgumentIndex(argumentList, argument);
			var argumentSymbol = methodSymbol.Parameters[index];

			if (argumentSymbol.OriginalDefinition.Type.TypeKind == TypeKind.TypeParameter)
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.UseNamedArgumentsWhenPassingBooleanLiteralsRule,
					argument.GetLocation()));
		}

		static int FindArgumentIndex(ArgumentListSyntax haystack, ArgumentSyntax needle)
		{
			var i = 0;
			var enumerator = haystack.Arguments.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == needle)
				{
					return i;
				}

				i++;
			}

			throw new InvalidOperationException("Failed to find Argument in its parent ArgumentList.");
		}
	}
}
