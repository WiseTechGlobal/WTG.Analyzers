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

		public async Task VerifyFixAsync(string oldSource, string newSource)
		{
			var document = ModelUtils.CreateDocument(oldSource);
			document = await FixDocumentAsync(document).ConfigureAwait(false);
			var actual = await GetReducedStringFromDocumentAsync(document).ConfigureAwait(false);
			Assert.That(actual, Is.EqualTo(newSource));
		}

		async Task<Document> FixDocumentAsync(Document document)
		{
			var analyzerDiagnostics = await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false);
			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var attempts = analyzerDiagnostics.Length;

			// keep applying fixes until all the problems go away (assuming an upper bound of one fix per issue.)
			for (var i = 0; i < attempts; ++i)
			{
				var actions = await RequestFixes(document, analyzerDiagnostics).ConfigureAwait(false);
				var actionToRun = actions.FirstOrDefault();

				if (actionToRun == null)
				{
					break;
				}

				document = await ApplyFixAsync(document, actionToRun).ConfigureAwait(false);

				analyzerDiagnostics = await DiagnosticUtils.GetDiagnosticsAsync(Analyzer, new[] { document }).ConfigureAwait(false);

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

				if (newCompilerDiagnostics.Any())
				{
					await ReportNewCompilerDiagnosticAsync(document, compilerDiagnostics).ConfigureAwait(false);
				}

				if (!analyzerDiagnostics.Any())
				{
					break;
				}
			}

			return document;
		}

		async Task<List<CodeAction>> RequestFixes(Document document, Diagnostic[] diagnostics)
		{
			var actions = new List<CodeAction>();
			var context = new CodeFixContext(document, diagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
			await CodeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);
			return actions;
		}

		static async Task ReportNewCompilerDiagnosticAsync(Document document, IEnumerable<Diagnostic> existingCompilerDiagnostics)
		{
			document = document.WithSyntaxRoot(await FormatTree(document).ConfigureAwait(false));

			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var newCompilerDiagnostics = GetNewDiagnostics(existingCompilerDiagnostics, compilerDiagnostics);
			var tree = await document.GetSyntaxRootAsync().ConfigureAwait(false);

			Assert.Fail("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
				string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
				tree.ToFullString());
		}

		static async Task<Document> ApplyFixAsync(Document document, CodeAction codeAction)
		{
			var operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
			var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			return solution.GetDocument(document.Id);
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
			return semanticModel.GetDiagnostics();
		}

		static async Task<string> GetReducedStringFromDocumentAsync(Document document)
		{
			var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation).ConfigureAwait(false);
			var root = await FormatTree(simplifiedDoc).ConfigureAwait(false);
			return root.GetText().ToString();
		}

		static async Task<SyntaxNode> FormatTree(Document document)
		{
			var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
			return Formatter.Format(root, Formatter.Annotation, document.Project.Solution.Workspace);
		}
	}
}

