using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
			List<Location>? brokenIndentation = null;
			List<Location>? incorrectEOL = null;

			foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
			{
				switch (trivia.Kind())
				{
					case SyntaxKind.EndOfLineTrivia:
						if (TryGetPrecedingTrivia(trivia, out var precedingTrivia) && precedingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
						{
							context.ReportDiagnostic(Rules.CreateDoNotLeaveWhitespaceOnTheEndOfTheLineDiagnostic(precedingTrivia.GetLocation()));
						}

						if (trivia.ToString() != Environment.NewLine)
						{
							incorrectEOL ??= new List<Location>();
							incorrectEOL.Add(trivia.GetLocation());
						}
						break;

					case SyntaxKind.WhitespaceTrivia:
						{
							var location = trivia.GetLocation();

							if (location.GetLineSpan().StartLinePosition.Character == 0 &&
								!acceptableLeadingWhitespace.IsMatch(trivia.ToString()))
							{
								brokenIndentation ??= new List<Location>();
								brokenIndentation.Add(location);
							}
						}
						break;

					case SyntaxKind.DocumentationCommentExteriorTrivia:
						{
							var location = trivia.GetLocation();

							if (location.GetLineSpan().StartLinePosition.Character == 0)
							{
								var text = LeadingWhitespace(trivia.ToString());

								if (!acceptableLeadingWhitespace.IsMatch(text))
								{
									var start = location.SourceSpan.Start;
									brokenIndentation ??= new List<Location>();
									brokenIndentation.Add(Location.Create(location.SourceTree, TextSpan.FromBounds(start, start + text.Length)));
								}
							}
						}
						break;
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

			if (incorrectEOL != null)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Rules.UseConsistentLineEndingsRule,
						incorrectEOL[0],
						incorrectEOL.Skip(1),
						humanReadablePlatformNewLine));
			}
		}

		static bool TryGetPrecedingTrivia(SyntaxTrivia trivia, out SyntaxTrivia precedingTrivia)
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
					precedingTrivia = default;
					return false;
				}
			}

			if (index > 0)
			{
				precedingTrivia = list[index - 1];
				return true;
			}

			precedingTrivia = default;
			return false;
		}

		static string LeadingWhitespace(string text)
		{
			for (var n = 0; n < text.Length; n++)
			{
				if (!char.IsWhiteSpace(text, n))
				{
					return text.Substring(0, n);
				}
			}

			return text;
		}

		// Must consist of one or more tab followed by up to 3 spaces.
		// (sometimes visual studio likes to add a few spaces to spaces to align with something on the previous line.)
		static readonly Regex acceptableLeadingWhitespace = new Regex(@"^\t*[ ]{0,3}$", RegexOptions.ExplicitCapture);

		static readonly string humanReadablePlatformNewLine = Environment.NewLine switch
		{
			"\r\n" => "CRLF",
			"\r" => "CR",
			"\n" => "LF",
			_ => throw new PlatformNotSupportedException(),
		};
	}
}
