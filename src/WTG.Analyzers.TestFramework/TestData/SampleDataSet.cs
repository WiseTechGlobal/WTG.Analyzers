using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers.TestFramework
{
	public sealed class SampleDataSet
	{
		SampleDataSet(string name, LanguageVersion languageVersion, string source, string result, ImmutableArray<DiagnosticResult> diagnostics, ImmutableHashSet<string> suppressedIds, ImmutableArray<OSPlatform> platforms)
		{
			Name = name;
			LanguageVersion = languageVersion;
			Source = source;
			Result = result;
			Diagnostics = diagnostics;
			SuppressedIds = suppressedIds;
			Platforms = platforms;
		}

		public string Name { get; }
		public LanguageVersion LanguageVersion { get; }
		public string Source { get; }
		public string Result { get; }
		public ImmutableArray<DiagnosticResult> Diagnostics { get; }
		public ImmutableHashSet<string> SuppressedIds { get; }
		public ImmutableArray<OSPlatform> Platforms { get; }

		public override string ToString() => Name;

		public bool IsApplicableToCurrentPlatform() => Platforms.IsDefaultOrEmpty || Platforms.Any(p => RuntimeInformation.IsOSPlatform(p));

		public static ImmutableArray<SampleDataSet> GetSamples(Assembly assembly, string prefix)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			var tmp =
				from name in assembly.GetManifestResourceNames()
				where name.StartsWith(prefix, StringComparison.Ordinal)
				let index = FileNameStartIndex(prefix, name)
				where index >= 0
				let sampleName = name.Substring(prefix.Length, index - prefix.Length)
				group new KeyValuePair<string, string>(name.Substring(index + 1), name) by sampleName into g
				let sampleData = GetSampleData(assembly, g.Key, g)
				where sampleData.IsApplicableToCurrentPlatform()
				select sampleData;

			return tmp.ToImmutableArray();
		}

		static int FileNameStartIndex(string prefix, string name)
		{
			var index = name.LastIndexOf('.', name.Length - 1, name.Length - prefix.Length);

			if (index < 0)
			{
				return index;
			}

			return name.LastIndexOf('.', index - 1, index - prefix.Length);
		}

		static SampleDataSet GetSampleData(Assembly assembly, string name, IEnumerable<KeyValuePair<string, string>> resourceNames)
		{
			string? source = null;
			string? result = null;
			var diagnostics = ImmutableArray<DiagnosticResult>.Empty;
			var suppressedIds = ImmutableHashSet<string>.Empty;
			var languageVersion = LanguageVersion.Default;
			var platforms = ImmutableArray<OSPlatform>.Empty;

			foreach (var pair in resourceNames)
			{
				switch (pair.Key)
				{
					case "Source.cs":
						source = LoadResource(assembly, pair.Value);
						break;

					case "Result.cs":
						result = LoadResource(assembly, pair.Value);
						break;

					case "Diagnostics.xml":
						LoadResults(assembly, pair.Value, ref languageVersion, ref diagnostics, ref suppressedIds, ref platforms);
						break;
				}
			}

			return new SampleDataSet(
				name,
				languageVersion,
				source ?? string.Empty,
				result ?? source ?? string.Empty,
				diagnostics,
				suppressedIds,
				platforms);
		}

		static string? LoadResource(Assembly assembly, string name)
		{
			using var stream = assembly.GetManifestResourceStream(name);

			if (stream == null)
			{
				return null;
			}

			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		static void LoadResults(Assembly assembly, string name, ref LanguageVersion languageVersion, ref ImmutableArray<DiagnosticResult> diagnostics, ref ImmutableHashSet<string> suppressedIds, ref ImmutableArray<OSPlatform> platforms)
		{
			var text = LoadResource(assembly, name);

			if (!string.IsNullOrEmpty(text))
			{
				var root = XElement.Parse(text);
				diagnostics = root.Descendants("diagnostic").Select(LoadResult).ToImmutableArray();
				suppressedIds = root.Elements("suppressId").Select(x => x.Value).ToImmutableHashSet();
				platforms = root.Elements("platform").Select(x => OSPlatform.Create(x.Value)).ToImmutableArray();

				var languageVersionStr = root.Element("languageVersion")?.Value;

				if (LanguageVersionFacts.TryParse(languageVersionStr, out var tmp))
				{
					languageVersion = tmp;
				}
			}
		}

		static DiagnosticResult LoadResult(XElement element)
		{
			var id = GetStringValue(element, "id") ?? throw new FormatException("Could not find a suitable 'id' attribute.");
			var message = GetStringValue(element, "message") ?? throw new FormatException("Could not find a suitable 'message' attribute.");

			return new DiagnosticResult(
				id,
				GetEnumValue<DiagnosticSeverity>(element, "severity"),
				message,
				element.Elements("location").Select(LoadLocation).ToImmutableArray());
		}

		static T GetEnumValue<T>(XElement element, string name)
			where T : struct
		{
			return (T)Enum.Parse(typeof(T), GetStringValue(element, name));
		}

		static string? GetStringValue(XElement element, string name)
		{
			while (element != null)
			{
				var att = element.Attribute(name);

				if (att != null)
				{
					return att.Value;
				}

				element = element.Parent;
			}

			return null;
		}

		static DiagnosticResultLocation LoadLocation(XElement element)
		{
			const string Pattern = @"(?<path>[a-z0-9.]+):\s*\((?<startLine>[0-9]+),\s*(?<startColumn>[0-9]+)(\)\s*-\s*\((?<endLine>[0-9]+),\s*(?<endColumn>[0-9]+)|(-(?<endColumn>[0-9]+))?)\)";
			var match = Regex.Match(element.Value, Pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

			var startLine = int.Parse(match.Groups["startLine"].Value, CultureInfo.InvariantCulture);
			var startColumn = int.Parse(match.Groups["startColumn"].Value, CultureInfo.InvariantCulture);

			var endLineGroup = match.Groups["endLine"];
			var endColumnGroup = match.Groups["endColumn"];

			var endLine = endLineGroup.Success ? int.Parse(endLineGroup.Value, CultureInfo.InvariantCulture) : startLine;
			var endColumn = endColumnGroup.Success ? int.Parse(endColumnGroup.Value, CultureInfo.InvariantCulture) : startColumn;

			return new DiagnosticResultLocation(
				match.Groups["path"].Value,
				startLine,
				startColumn,
				endLine,
				endColumn);
		}
	}
}
