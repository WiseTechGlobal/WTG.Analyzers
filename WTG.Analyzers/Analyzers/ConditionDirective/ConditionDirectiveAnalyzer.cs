using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class ConditionDirectiveAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.ConditionalCompilationDirectivesShouldNotSplitStructuresRule,
			Rules.AvoidConditionalCompilationBasedOnDebugRule);

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
			var visitor = new ConditionValidator(context.ReportDiagnostic);

			foreach (var condition in ConditionDirective.Extract(root))
			{
				visitor.Visit(condition.If.GetStructure());
				var previous = condition.If;

				foreach (var current in condition.ElseIf)
				{
					visitor.Visit(current.GetStructure());
					CheckConditionalCompilationDirectivePair(context, previous, current);
					previous = current;
				}

				if (!condition.Else.IsKind(SyntaxKind.None))
				{
					CheckConditionalCompilationDirectivePair(context, previous, condition.Else);
					previous = condition.Else;
				}

				CheckConditionalCompilationDirectivePair(context, previous, condition.End);
			}
		}

		static void CheckConditionalCompilationDirectivePair(SyntaxTreeAnalysisContext context, SyntaxTrivia start, SyntaxTrivia end)
		{
			if (DirectiveHelper.BoundingTriviaSplitsNodes(start, end))
			{
				context.ReportDiagnostic(Rules.CreateConditionalCompilationDirectivesShouldNotSplitStructuresDiagnostic(start.GetLocation()));
			}
		}

		static IEnumerable<SyntaxTrivia> TriviaSequence(ConditionDirective directive)
		{
			yield return directive.If;

			foreach (var trivia in directive.ElseIf)
			{
				yield return trivia;
			}

			if (!directive.Else.IsKind(SyntaxKind.None))
			{
				yield return directive.Else;
			}

			yield return directive.End;
		}

		sealed class ConditionValidator : CSharpSyntaxWalker
		{
			public ConditionValidator(Action<Diagnostic> report)
			{
				this.report = report;
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				if (node.Identifier.Text == "DEBUG")
				{
					report(Rules.CreateAvoidConditionalCompilationBasedOnDebugDiagnostic(node.Identifier.GetLocation()));
				}
			}

			readonly Action<Diagnostic> report;
		}
	}
}
