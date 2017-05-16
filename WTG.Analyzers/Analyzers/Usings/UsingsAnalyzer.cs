using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class UsingsAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UsingDirectivesMustBeOrderedByKindRule);

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
				SyntaxKind.CompilationUnit,
				SyntaxKind.NamespaceDeclaration);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var usingsList = UsingsHelper.ExtractUsings(context.Node);
			var enumerator = usingsList.GetEnumerator();

			if (enumerator.MoveNext())
			{
				var previousUsing = enumerator.Current;

				while (enumerator.MoveNext())
				{
					var currentUsing = enumerator.Current;
					
					var comparisonResult = Compare(previousUsing, currentUsing);
					if (comparisonResult > 0)
					{
						context.ReportDiagnostic(Diagnostic.Create(Rules.UsingDirectivesMustBeOrderedByKindRule, currentUsing.GetLocation()));
						return;
					}

					previousUsing = currentUsing;
				}
			}
		}

		static int Compare(UsingDirectiveSyntax first, UsingDirectiveSyntax second)
		{
			var firstKind = UsingsHelper.GetUsingDirectiveKind(first);
			var secondKind = UsingsHelper.GetUsingDirectiveKind(second);

			return firstKind.CompareTo(secondKind);
		}
	}
}
