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
			var document = CreateDocument(dataSet.Source, (dataSet.Options & SampleDataSetOptions.OmitAssemblyReferences) != 0);
			var project = document.Project;

			project = project.WithParseOptions(GetParseOptions(project).WithLanguageVersion(dataSet.LanguageVersion));

			if ((dataSet.Options & SampleDataSetOptions.DisableNRT) == 0)
			{
				var effectiveVersion = LanguageVersionFacts.MapSpecifiedToEffectiveVersion(dataSet.LanguageVersion);

				if (effectiveVersion >= LanguageVersion.CSharp8)
				{
					project = project.WithCompilationOptions(GetCompilationOptions(project).WithNullableContextOptions(NullableContextOptions.Enable));
				}
			}

			return project.GetDocument(document.Id)!;
		}

		public static Document CreateDocument(string source) => CreateDocument(source, omitAssemblyReferences: false);

		public static Document CreateDocument(string source, bool omitAssemblyReferences)
		{
			return CreateProject(new[] { source }, omitAssemblyReferences).Documents.First();
		}

		public static Document[] CreateDocument(params string[] sources) => CreateDocument(sources, omitAssemblyReferences: false);

		public static Document[] CreateDocument(string[] sources, bool omitAssemblyReferences)
		{
			var project = CreateProject(sources, omitAssemblyReferences);
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
			var solution = workspace.CurrentSolution;

			workspace.TryApplyChanges(
				solution.WithOptions(
					solution.Options
					.WithChangedOption(new OptionKey(FormattingOptions.NewLine, LanguageNames.CSharp), Environment.NewLine)
					.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
					.WithChangedOption(new OptionKey(FormattingOptions.TabSize, LanguageNames.CSharp), 2)
					.WithChangedOption(new OptionKey(FormattingOptions.IndentationSize, LanguageNames.CSharp), 2)));

			return workspace;
		}

		public static Project CreateProject(params string[] sources) => CreateProject(sources, omitAssemblyReferences: false);

		public static Project CreateProject(string[] sources, bool omitAssemblyReferences)
		{
#pragma warning disable CA2000 // Dispose objects before losing scope
			return AddProject(CreateWorkspace().CurrentSolution, TestProjectName, sources, omitAssemblyReferences);
#pragma warning restore CA2000 // Dispose objects before losing scope
		}

		public static Project AddAdHocDependency(this Project project, string assemblyName, params string[] sources)
		{
			var newProject = AddProject(project.Solution, assemblyName, sources, omitAssemblyReferences: false);

			return newProject.Solution.GetProject(project.Id)!
				.WithProjectReferences(project.ProjectReferences.Concat(new[] { new ProjectReference(newProject.Id) }));
		}

		static Project AddProject(Solution currentSolution, string assemblyName, string[] sources, bool omitAssemblyReferences)
		{
			var projectId = ProjectId.CreateNewId(debugName: assemblyName);

			var solution = currentSolution.AddProject(projectId, assemblyName, assemblyName, LanguageNames.CSharp);

			var project = solution.GetProject(projectId)!;

			if (!omitAssemblyReferences)
			{
				project = project.AddMetadataReferences(MetadataReferences);
			}

			var compilationOptions = GetCompilationOptions(project)
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

			return solution.GetProject(projectId)!;
		}

		static string GetFileName(int count)
		{
			return DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
		}

		static CSharpParseOptions GetParseOptions(Project project) => (CSharpParseOptions)project.ParseOptions!;
		static CSharpCompilationOptions GetCompilationOptions(Project project) => (CSharpCompilationOptions)project.CompilationOptions!;

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
