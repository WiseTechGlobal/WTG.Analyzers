using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WTG.Analyzers.TestFramework
{
	public static partial class DiagnosticUtils
	{
		public static async Task<Diagnostic[]> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, params string[] sources)
		{
			return await GetDiagnosticsAsync(analyzer, ModelUtils.GetDocuments(sources)).ConfigureAwait(false);
		}

		public static async Task<Diagnostic[]> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, params Document[] documents)
		{
			var ids = new HashSet<string>(analyzer.SupportedDiagnostics.Select(x => x.Id));
			var projects = new HashSet<Project>();

			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var diagnostics = new List<Diagnostic>();

			foreach (var p in projects)
			{
				var project = p;
				if (project.ParseOptions.Language == LanguageNames.CSharp)
				{
					var options = ((CSharpParseOptions)project.ParseOptions).WithLanguageVersion(LanguageVersion.Latest);
					project = project.WithParseOptions(options);
				}

				var complation = await project.GetCompilationAsync().ConfigureAwait(false);
				var compilationWithAnalyzers = complation.WithAnalyzers(ImmutableArray.Create(analyzer));
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
							if (project.GetDocument(diag.Location.SourceTree).Id == document.Id)
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

			return descriptor.DefaultSeverity == DiagnosticSeverity.Error
				&& descriptor.CustomTags.Any(x => CriticalTags.Contains(x));
		}

		static readonly ImmutableHashSet<string> CriticalTags = ImmutableHashSet.Create(
			WellKnownDiagnosticTags.Build,
			WellKnownDiagnosticTags.Compiler,
			WellKnownDiagnosticTags.AnalyzerException);
	}
}
