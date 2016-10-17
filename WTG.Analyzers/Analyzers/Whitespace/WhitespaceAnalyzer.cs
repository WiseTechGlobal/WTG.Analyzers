using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class WhitespaceAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new[]
		{
			Rules.DoNotLeaveWhitespaceOnTheEndOfTheLineRule,
		});

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxTreeAction(Analyze);
		}

		static void Analyze(SyntaxTreeAnalysisContext context)
		{
			if (context.Tree.IsGenerated(context.CancellationToken))
			{
				return;
			}

			var root = context.Tree.GetRoot(context.CancellationToken);

			foreach (var trivia in root.DescendantTrivia())
			{
				if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
				{
					SyntaxTrivia preceedingTrivia;

					if (TryGetPreceedingTrivia(trivia, out preceedingTrivia) && preceedingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
					{
						context.ReportDiagnostic(Rules.CreateDoNotLeaveWhitespaceOnTheEndOfTheLineDiagnostic(preceedingTrivia.GetLocation()));
					}
				}
			}
		}

		static bool TryGetPreceedingTrivia(SyntaxTrivia trivia, out SyntaxTrivia preceedingTrivia)
		{
			var token = trivia.Token;
			var list = token.TrailingTrivia;
			var index = list.IndexOf(trivia);

			if (index < 0)
			{
				list = token.LeadingTrivia;
				index = list.IndexOf(trivia);

				if (index < 0)
				{
					preceedingTrivia = default(SyntaxTrivia);
					return false;
				}
			}

			if (index > 0)
			{
				preceedingTrivia = list[index - 1];
				return true;
			}

			preceedingTrivia = default(SyntaxTrivia);
			return false;
		}
	}
}
