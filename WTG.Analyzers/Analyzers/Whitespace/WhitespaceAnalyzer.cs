using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class WhitespaceAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Rules.DoNotLeaveWhitespaceOnTheEndOfTheLineRule,
			Rules.IndentWithTabsRatherThanSpacesRule,
			Rules.UseConsistentLineEndingsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxTreeAction(Analyze);
		}

		static void Analyze(SyntaxTreeAnalysisContext context)
		{
			if (context.Tree.IsGenerated(context.CancellationToken))
			{
				return;
			}

			var root = context.Tree.GetRoot(context.CancellationToken);
			List<Location> brokenIndentation = null;

			foreach (var trivia in root.DescendantTrivia())
			{
				if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
				{
					SyntaxTrivia preceedingTrivia;

					if (TryGetPreceedingTrivia(trivia, out preceedingTrivia) && preceedingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
					{
						context.ReportDiagnostic(Rules.CreateDoNotLeaveWhitespaceOnTheEndOfTheLineDiagnostic(preceedingTrivia.GetLocation()));
					}

					if (trivia.ToString() != "\r\n")
					{
						context.ReportDiagnostic(Rules.CreateUseConsistentLineEndingsDiagnostic(trivia.GetLocation()));
					}
				}
				else if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
				{
					var location = trivia.GetLocation();

					if (location.GetLineSpan().StartLinePosition.Character == 0 &&
						!acceptableLeadingWhitespace.IsMatch(trivia.ToString()))
					{
						if (brokenIndentation == null)
						{
							brokenIndentation = new List<Location>();
						}

						brokenIndentation.Add(location);
					}
				}
			}

			if (brokenIndentation != null)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Rules.IndentWithTabsRatherThanSpacesRule,
						brokenIndentation[0],
						brokenIndentation.Skip(1)));
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

		// Must consist of one or more tab followed by up to 3 spaces.
		// (sometimes visual studio likes to add a few spaces to spaces to align with something on the previous line.)
		static readonly Regex acceptableLeadingWhitespace = new Regex(@"^\t*[ ]{0,3}$", RegexOptions.ExplicitCapture);
	}
}
