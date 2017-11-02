using System;
using System.Linq;
using System.Reflection;
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

		public static AdhocWorkspace CreateWorkspace()
		{
			var workspace = new AdhocWorkspace();
			workspace.Options = workspace.Options
				.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(new OptionKey(FormattingOptions.TabSize, LanguageNames.CSharp), 2)
				.WithChangedOption(new OptionKey(FormattingOptions.IndentationSize, LanguageNames.CSharp), 2);
			return workspace;
		}

		public static Project CreateProject(params string[] sources)
		{
			return AddProject(CreateWorkspace().CurrentSolution, TestProjectName, sources);
		}

		public static Project AddAdHocDependency(this Project project, string assemblyName, params string[] sources)
		{
			var newProject = AddProject(project.Solution, assemblyName, sources);

			return newProject.Solution.GetProject(project.Id)
				.WithProjectReferences(project.ProjectReferences.Concat(new[] { new ProjectReference(newProject.Id) }));
		}

		static Project AddProject(Solution currentSolution, string assemblyName, string[] sources)
		{
			var projectId = ProjectId.CreateNewId(debugName: assemblyName);

			var solution = currentSolution
				.AddProject(projectId, assemblyName, assemblyName, LanguageNames.CSharp)
				.AddMetadataReference(projectId, CorlibReference)
				.AddMetadataReference(projectId, SystemCoreReference)
				.AddMetadataReference(projectId, CSharpSymbolsReference)
				.AddMetadataReference(projectId, CodeAnalysisReference);

			solution = solution.WithProjectCompilationOptions(projectId, solution.GetProject(projectId).CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

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

		static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
		static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location);
		static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).GetTypeInfo().Assembly.Location);
		static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).GetTypeInfo().Assembly.Location);
	}
}
