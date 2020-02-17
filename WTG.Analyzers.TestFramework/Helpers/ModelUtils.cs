using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace WTG.Analyzers.TestFramework
{
	public static class ModelUtils
	{
		public static Document CreateDocument(SampleDataSet dataSet)
		{
			var document = CreateDocument(dataSet.Source);
			var project = document.Project;

			project = project.WithParseOptions(
				((CSharpParseOptions)project.ParseOptions).WithLanguageVersion(dataSet.LanguageVersion));

			return project.GetDocument(document.Id);
		}

		public static Document CreateDocument(string source)
		{
			return CreateProject(new[] { source }).Documents.First();
		}

		public static Document[] CreateDocument(params string[] sources)
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
#pragma warning disable CA2000 // Dispose objects before losing scope
			return AddProject(CreateWorkspace().CurrentSolution, TestProjectName, sources);
#pragma warning restore CA2000 // Dispose objects before losing scope
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

			var solution = currentSolution.AddProject(projectId, assemblyName, assemblyName, LanguageNames.CSharp);

			var project = solution.GetProject(projectId)
				.AddMetadataReferences(MetadataReferences);

			var compilationOptions = (CSharpCompilationOptions)project.CompilationOptions;
			compilationOptions = compilationOptions
				.WithAllowUnsafe(enabled: true)
				.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
			project = project.WithCompilationOptions(compilationOptions);

			solution = project.Solution;

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

		static readonly ImmutableArray<MetadataReference> MetadataReferences = GetMetadataReferences();

		static ImmutableArray<MetadataReference> GetMetadataReferences()
		{
			var types = new[]
			{
				typeof(object),
				typeof(Regex),
				typeof(Enumerable),
				typeof(CSharpCompilation),
				typeof(Compilation),
			};

			var wellKnownAssemblyNames = new[]
			{
				"mscorlib",
				"System.Collections",
				"System.Collections.Concurrent",
				"System.Console",
				"System.Linq.Expressions",
				"System.Linq.Queryable",
				"System.Runtime",
			};

			var builder = ImmutableArray.CreateBuilder<MetadataReference>(initialCapacity: types.Length + wellKnownAssemblyNames.Length);

			foreach (var type in types)
			{
				var reference = MetadataReference.CreateFromFile(type.GetTypeInfo().Assembly.Location);
				builder.Add(reference);
			}

			foreach (var assemblyName in wellKnownAssemblyNames)
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.Load(assemblyName);
				}
				catch (FileNotFoundException)
				{
					// Not all of these assemblies are available (or required) on all runtimes.
					continue;
				}

				var reference = MetadataReference.CreateFromFile(assembly.Location);
				builder.Add(reference);
			}

			return builder.ToImmutable();
		}
	}
}
