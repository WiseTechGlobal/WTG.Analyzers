using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WTG.Analyzers.Test.Helpers
{
	public abstract partial class DiagnosticUtils
	{
		public static async Task<Diagnostic[]> GetSortedDiagnosticsAsync(DiagnosticAnalyzer analyzer, params string[] sources)
		{
			return await GetSortedDiagnosticsAsync(analyzer, ModelUtils.GetDocuments(sources)).ConfigureAwait(false);
		}

		public static async Task<Diagnostic[]> GetSortedDiagnosticsAsync(DiagnosticAnalyzer analyzer, Document[] documents)
		{
			var ids = new HashSet<string>(analyzer.SupportedDiagnostics.Select(x => x.Id));
			var projects = new HashSet<Project>();

			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var diagnostics = new List<Diagnostic>();

			foreach (var project in projects)
			{
				var complation = await project.GetCompilationAsync().ConfigureAwait(false);
				var compilationWithAnalyzers = complation.WithAnalyzers(ImmutableArray.Create(analyzer));
				var diags = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

				foreach (var diag in diags.Where(x => ids.Contains(x.Id)))
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						diagnostics.Add(diag);
					}
					else
					{
						for (int i = 0; i < documents.Length; i++)
						{
							var document = documents[i];
							var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);

							if (tree == diag.Location.SourceTree)
							{
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			return SortDiagnostics(diagnostics);
		}

		static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
		{
			return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
		}
	}
}
