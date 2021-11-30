using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace WTG.Analyzers.Utils
{
	public abstract class DocumentBatchedFixAllProvider : FixAllProvider
	{
		protected DocumentBatchedFixAllProvider()
		{
		}

		const string Title = "<fix-all>";

		public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
		{
			if (fixAllContext.Document != null)
			{
				switch (fixAllContext.Scope)
				{
					case FixAllScope.Document:
						return GetFixForDocumentAsync(fixAllContext);
					case FixAllScope.Project:
						return GetFixForProjectAsync(fixAllContext);
					case FixAllScope.Solution:
						return GetFixForSolutionAsync(fixAllContext);
				}
			}

			var solution = fixAllContext.Solution;
			var codeActionEquivalenceKey = fixAllContext.CodeActionEquivalenceKey;
			var codeAction = CodeAction.Create(Title, c => Task.FromResult(solution), codeActionEquivalenceKey);
			return Task.FromResult(codeAction);
		}

		/// <summary>
		/// Semantic information should only be gotten from originalDocument (to avoid recompilation),
		/// but we should make our changes to documentToFix (so that we don't discard fixes made to other documents in the project).
		/// If we only ever make changes to documentToFix, then the syntax trees for the two documents will be identical on entry.
		/// </summary>
		protected abstract Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken);

		protected virtual async Task<Solution> ApplyFixesAsync(Solution solution, ImmutableDictionary<Document, ImmutableArray<Diagnostic>> diagnostics, CancellationToken cancellationToken)
		{
			foreach (var pair in diagnostics)
			{
				var originalDocument = pair.Key;
				var documentToFix = solution.GetDocument(originalDocument.Id);

				if (documentToFix != null)
				{
					var newDocument = await ApplyFixesAsync(originalDocument, documentToFix, pair.Value, cancellationToken).ConfigureAwait(false);
					solution = newDocument.Project.Solution;
				}
			}

			return solution;
		}

		async Task<CodeAction> GetFixForDocumentAsync(FixAllContext fixAllContext)
		{
			var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
			return CreateFixAction(fixAllContext.Document, diagnostics, fixAllContext.CodeActionEquivalenceKey);
		}

		async Task<CodeAction> GetFixForProjectAsync(FixAllContext fixAllContext)
		{
			var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(fixAllContext.Project).ConfigureAwait(false);

			var builder = new Dictionary<Document, ImmutableArray<Diagnostic>.Builder>();
			PopulateDiagnosticDictionary(builder, fixAllContext.Project, diagnostics);
			var groupedDiagnostics = builder.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutable());

			return CreateFixAction(fixAllContext.Solution, groupedDiagnostics, fixAllContext.CodeActionEquivalenceKey);
		}

		async Task<CodeAction> GetFixForSolutionAsync(FixAllContext fixAllContext)
		{
			var solution = fixAllContext.Solution;
			var triggerProject = fixAllContext.Project;
			var builder = new Dictionary<Document, ImmutableArray<Diagnostic>.Builder>();

			foreach (var project in solution.Projects)
			{
				if (project.Language == triggerProject.Language)
				{
					var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
					PopulateDiagnosticDictionary(builder, project, diagnostics);
				}
			}

			var groupedDiagnostics = builder.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutable());
			return CreateFixAction(solution, groupedDiagnostics, fixAllContext.CodeActionEquivalenceKey);
		}

		CodeAction CreateFixAction(Document document, ImmutableArray<Diagnostic> diagnostics, string codeActionEquivalenceKey)
		{
			return CodeAction.Create(
				Title,
				createChangedDocument: c => ApplyFixesAsync(document, document, diagnostics, c),
				equivalenceKey: codeActionEquivalenceKey);
		}

		CodeAction CreateFixAction(Solution solution, ImmutableDictionary<Document, ImmutableArray<Diagnostic>> diagnostics, string codeActionEquivalenceKey)
		{
			return CodeAction.Create(
				Title,
				createChangedSolution: c => ApplyFixesAsync(solution, diagnostics, c),
				equivalenceKey: codeActionEquivalenceKey);
		}

		static void PopulateDiagnosticDictionary(Dictionary<Document, ImmutableArray<Diagnostic>.Builder> builder, Project project, ImmutableArray<Diagnostic> diagnostics)
		{
			foreach (var diagnostic in diagnostics)
			{
				var document = project.GetDocument(diagnostic.Location.SourceTree)
					?? throw new ArgumentException("Diagnostic not from specified project.");

				if (!builder.TryGetValue(document, out var list))
				{
					builder.Add(document, list = ImmutableArray.CreateBuilder<Diagnostic>());
				}

				list.Add(diagnostic);
			}
		}
	}
}
