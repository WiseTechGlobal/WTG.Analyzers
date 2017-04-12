using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace WTG.Analyzers.TestFramework
{
	public static class ModelUtils
	{
		public static Document CreateDocument(string source)
		{
			return CreateProject(new[] { source }).Documents.First();
		}

		public static Document[] GetDocuments(string[] sources)
		{
			var project = CreateProject(sources);
			var documents = project.Documents.ToArray();

			if (documents.Length != sources.Length)
			{
				throw new InvalidOperationException("The number of Documents created did not match the number of sources provided.");
			}

			return documents;
		}

		static Project CreateProject(string[] sources)
		{
			var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

			var workspace = new AdhocWorkspace();
			workspace.Options = workspace.Options
				.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(new OptionKey(FormattingOptions.TabSize, LanguageNames.CSharp), 2)
				.WithChangedOption(new OptionKey(FormattingOptions.IndentationSize, LanguageNames.CSharp), 2);

			var solution = workspace
				.CurrentSolution
				.AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
				.AddMetadataReference(projectId, CorlibReference)
				.AddMetadataReference(projectId, SystemCoreReference)
				.AddMetadataReference(projectId, CSharpSymbolsReference)
				.AddMetadataReference(projectId, CodeAnalysisReference);

			for (var i = 0; i < sources.Length; i++)
			{
				var newFileName = GetFileName(i);

				solution = solution.AddDocument(
					DocumentId.CreateNewId(projectId, debugName: newFileName),
					newFileName,
					SourceText.From(sources[i]));
			}

			return solution.GetProject(projectId);
		}

		static string GetFileName(int count)
		{
			return DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
		}

		const string DefaultFilePathPrefix = "Test";
		const string CSharpDefaultFileExt = "cs";
		const string TestProjectName = "TestProject";

		static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
		static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
	}
}
