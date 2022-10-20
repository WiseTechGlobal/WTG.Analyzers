using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using NUnit.Framework;

namespace WTG.Analyzers.TestFramework
{
	public sealed class CodeFixer
	{
		public CodeFixer(DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider)
		{
			Analyzer = analyzer;
			CodeFixProvider = codeFixProvider;
		}

		public DiagnosticAnalyzer Analyzer { get; }
		public CodeFixProvider CodeFixProvider { get; }
		public Func<Diagnostic, bool>? DiagnosticFilter { get; set; }

		public async Task VerifyFixAsync(Document document, string newSource)
		{
			document = await FixDocumentAsync(document).ConfigureAwait(false);
			var actual = await GetReducedStringFromDocumentAsync(document).ConfigureAwait(false);
			Assert.That(actual, Is.EqualTo(newSource));
		}

		public async Task VerifyBulkFixAsync(Document document, string newSource)
		{
			document = await BulkFixDocumentAsync(document).ConfigureAwait(false);
			var actual = await GetReducedStringFromDocumentAsync(document).ConfigureAwait(false);
			Assert.That(actual, Is.EqualTo(newSource));
		}

		async Task<Document> FixDocumentAsync(Document document)
		{
			var analyzerDiagnostics = FilterDiagnostics(await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false));
			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var attempts = analyzerDiagnostics.Length;

			// keep applying fixes until all the problems go away (assuming an upper bound of one fix per issue.)
			for (var i = 0; i < attempts; ++i)
			{
				CodeAction? actionToRun = null;
				var diagnosticWithNoActionCount = 0;

				while (actionToRun == null && diagnosticWithNoActionCount < analyzerDiagnostics.Length)
				{
					var actions = await RequestFixes(document, analyzerDiagnostics[diagnosticWithNoActionCount]).ConfigureAwait(false);
					actionToRun = actions.FirstOrDefault();

					if (actionToRun != null)
					{
						document = await ApplyFixAsync(document, actionToRun).ConfigureAwait(false);
					}
					else
					{
						diagnosticWithNoActionCount++;
					}
				}

				analyzerDiagnostics = FilterDiagnostics(await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false));

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

				var filter = DiagnosticFilter;
				var hasMatchingCompilerDiagnostics = filter == null ? newCompilerDiagnostics.Any() : newCompilerDiagnostics.Any(filter);

				if (hasMatchingCompilerDiagnostics)
				{
					await ReportNewCompilerDiagnosticAsync(document, compilerDiagnostics).ConfigureAwait(false);
				}

				if (analyzerDiagnostics.Length == diagnosticWithNoActionCount)
				{
					break;
				}
			}

			return document;
		}

		async Task<Document> BulkFixDocumentAsync(Document document)
		{
			var analyzerDiagnostics = FilterDiagnostics(await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false));
			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var batchCount = -1;

			while (analyzerDiagnostics.Length > 0)
			{
				var actions = await RequestAllFixes(document, analyzerDiagnostics).ConfigureAwait(false);
				var batcher = CodeFixProvider.GetFixAllProvider();

				var groupedDiagnostics = Enumerable.ToArray(
					from tuple in actions
					where !string.IsNullOrEmpty(tuple.Item2.EquivalenceKey)
					group tuple.Item1 by tuple.Item2.EquivalenceKey into g
					let count = g.Count()
					orderby count descending
					select g);

				if (groupedDiagnostics.Length == 0)
				{
					break;
				}

				if (batchCount >= 0)
				{
					Assert.That(groupedDiagnostics.Length, Is.LessThan(batchCount), "The number of batches should decrease each pass.");
				}

				batchCount = groupedDiagnostics.Length;

				var batch = groupedDiagnostics[0];

				var context = new FixAllContext(
					document,
					CodeFixProvider,
					FixAllScope.Document,
					batch.Key,
					batch.Select(x => x.Id).Distinct().ToArray(),
					new DummyFixAllDiagnosticProvider(batch.ToArray()),
					CancellationToken.None);

				var fix = await batcher.GetFixAsync(context).ConfigureAwait(false);

				if (fix == null)
				{
					return document;
				}

				document = await ApplyFixAsync(document, fix).ConfigureAwait(false);
				analyzerDiagnostics = FilterDiagnostics(await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false));

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

				var filter = DiagnosticFilter;

				if (filter == null ? newCompilerDiagnostics.Any() : newCompilerDiagnostics.Any(filter))
				{
					await ReportNewCompilerDiagnosticAsync(document, compilerDiagnostics).ConfigureAwait(false);
				}

				if (analyzerDiagnostics.Length == 0)
				{
					break;
				}
			}

			return document;
		}

		Diagnostic[] FilterDiagnostics(Diagnostic[] result)
		{
			var filter = DiagnosticFilter;

			if (filter != null)
			{
				result = result.Where(filter).ToArray();
			}

			return result;
		}

		async Task<List<CodeAction>> RequestFixes(Document document, Diagnostic diagnostic)
		{
			var actions = new List<CodeAction>();

			if (IsFixable(diagnostic))
			{
				var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
				await CodeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);
			}

			return actions;
		}

		async Task<List<Tuple<Diagnostic, CodeAction>>> RequestAllFixes(Document document, Diagnostic[] diagnostics)
		{
			var actions = new List<Tuple<Diagnostic, CodeAction>>();

			foreach (var diagnostic in diagnostics)
			{
				if (IsFixable(diagnostic))
				{
					await CodeFixUtils.CollectCodeActions(CodeFixProvider, document, diagnostic, actions).ConfigureAwait(false);
				}
			}

			return actions;
		}

		bool IsFixable(Diagnostic diagnostic) => CodeFixProvider.FixableDiagnosticIds.Contains(diagnostic.Id);

		static async Task ReportNewCompilerDiagnosticAsync(Document document, IEnumerable<Diagnostic> existingCompilerDiagnostics)
		{
			document = document.WithSyntaxRoot(await FormatTree(document).ConfigureAwait(false));

			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var newCompilerDiagnostics = GetNewDiagnostics(existingCompilerDiagnostics, compilerDiagnostics);
			var tree = await document.GetSyntaxRootAsync().ConfigureAwait(false);

			Assert.Fail(
				"Fix introduced new compiler diagnostics:{0}{1}{0}{0}New document:{0}{2}{0}",
				Environment.NewLine,
				string.Join(Environment.NewLine, newCompilerDiagnostics.Select(d => d.ToString())),
				tree?.ToFullString());
		}

		static async Task<Document> ApplyFixAsync(Document document, CodeAction codeAction)
		{
			var operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
			var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			return solution.GetDocument(document.Id)
				?? throw new NotSupportedException("Analyzer attempted to remove the document being fixed, this is not currently a supported situation.");
		}

		static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
		{
			var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
			var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

			var oldIndex = 0;
			var newIndex = 0;

			while (newIndex < newArray.Length)
			{
				if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
				{
					++oldIndex;
					++newIndex;
				}
				else
				{
					yield return newArray[newIndex++];
				}
			}
		}

		static async Task<IEnumerable<Diagnostic>> GetCompilerDiagnosticsAsync(Document document)
		{
			var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			return semanticModel?.GetDiagnostics() ?? Enumerable.Empty<Diagnostic>();
		}

		static async Task<string> GetReducedStringFromDocumentAsync(Document document)
		{
			var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation).ConfigureAwait(false);
			var root = await FormatTree(simplifiedDoc).ConfigureAwait(false);
			return root.GetText().ToString();
		}

		static async Task<SyntaxNode> FormatTree(Document document)
		{
			var formattedDocument = await Formatter.FormatAsync(document, Formatter.Annotation).ConfigureAwait(false);
			var root = await formattedDocument.GetSyntaxRootAsync().ConfigureAwait(false);
			return root ?? throw new InvalidOperationException("Formatted document has no root.");
		}

		sealed class DummyFixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
		{
			public DummyFixAllDiagnosticProvider(Diagnostic[] diagnostics)
			{
				this.diagnostics = Task.FromResult(diagnostics.AsEnumerable());
			}

			public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) => diagnostics;
			public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) => diagnostics;
			public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) => diagnostics;

			readonly Task<IEnumerable<Diagnostic>> diagnostics;
		}
	}
}

