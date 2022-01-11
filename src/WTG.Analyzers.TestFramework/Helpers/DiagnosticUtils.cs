using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WTG.Analyzers.TestFramework
{
	public static partial class DiagnosticUtils
	{
		public static async Task<Diagnostic[]> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, params Document[] documents)
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
				var compilation = await project.GetCompilationAsync().ConfigureAwait(false);

				if (compilation == null)
				{
					continue;
				}

				var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
				var diags = await compilationWithAnalyzers.GetAllDiagnosticsAsync().ConfigureAwait(false);

				foreach (var diag in diags.Where(x => ids.Contains(x.Id) || IsImportant(x)))
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						diagnostics.Add(diag);
					}
					else
					{
						for (var i = 0; i < documents.Length; i++)
						{
							var document = documents[i];
							if (project.GetDocument(diag.Location.SourceTree)?.Id == document.Id)
							{
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			return diagnostics.ToArray();
		}

		static bool IsImportant(Diagnostic diag)
		{
			var descriptor = diag.Descriptor;

			foreach (var tag in descriptor.CustomTags)
			{
				switch (tag)
				{
					case WellKnownDiagnosticTags.AnalyzerException:
						return true;

					case WellKnownDiagnosticTags.Build:
					case WellKnownDiagnosticTags.Compiler:
						if (descriptor.DefaultSeverity == DiagnosticSeverity.Error)
						{
							return true;
						}
						break;
				}
			}

			return false;
		}
	}
}
