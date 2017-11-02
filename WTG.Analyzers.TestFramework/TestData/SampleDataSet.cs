using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.TestFramework
{
	public sealed class SampleDataSet
	{
		SampleDataSet(string name, string source, string result, ImmutableArray<DiagnosticResult> diagnostics, ImmutableHashSet<string> suppressedIds)
		{
			Name = name;
			Source = source;
			Result = result;
			Diagnostics = diagnostics;
			SuppressedIds = suppressedIds;
		}

		public string Name { get; }
		public string Source { get; }
		public string Result { get; }
		public ImmutableArray<DiagnosticResult> Diagnostics { get; }
		public ImmutableHashSet<string> SuppressedIds { get; }

		public override string ToString() => Name;

		public static ImmutableArray<SampleDataSet> GetSamples(Assembly assembly, string prefix)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			var tmp =
				from name in assembly.GetManifestResourceNames()
				where name.StartsWith(prefix, StringComparison.Ordinal)
				let index = name.IndexOf('.', prefix.Length)
				where index >= 0
				let sampleName = name.Substring(prefix.Length, index - prefix.Length)
				group new KeyValuePair<string, string>(name.Substring(index + 1), name) by sampleName into g
				select GetSampleData(assembly, g.Key, g);

			return tmp.ToImmutableArray();
		}

		static SampleDataSet GetSampleData(Assembly assembly, string name, IEnumerable<KeyValuePair<string, string>> resourceNames)
		{
			string source = null;
			string result = null;
			var diagnostics = ImmutableArray<DiagnosticResult>.Empty;
			var suppressedIds = ImmutableHashSet<string>.Empty;

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
						LoadResults(assembly, pair.Value, ref diagnostics, ref suppressedIds);
						break;
				}
			}

			return new SampleDataSet(name, source ?? string.Empty, result ?? source ?? string.Empty, diagnostics, suppressedIds);
		}

		static string LoadResource(Assembly assembly, string name)
		{
			using (var stream = assembly.GetManifestResourceStream(name))
			{
				if (stream == null)
				{
					return null;
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		static void LoadResults(Assembly assembly, string name, ref ImmutableArray<DiagnosticResult> diagnostics, ref ImmutableHashSet<string> suppressedIds)
		{
			var text = LoadResource(assembly, name);

			if (!string.IsNullOrEmpty(text))
			{
				var root = XElement.Parse(text);
				diagnostics = root.Descendants("diagnostic").Select(LoadResult).ToImmutableArray();
				suppressedIds = root.Elements("suppressId").Select(x => x.Value).ToImmutableHashSet();
			}
		}

		static DiagnosticResult LoadResult(XElement element)
		{
			return new DiagnosticResult(
				GetStringValue(element, "id"),
				GetEnumValue<DiagnosticSeverity>(element, "severity"),
				GetStringValue(element, "message"),
				element.Elements("location").Select(LoadLocation).ToImmutableArray());
		}

		static T GetEnumValue<T>(XElement element, string name)
			where T : struct
		{
			return (T)Enum.Parse(typeof(T), GetStringValue(element, name));
		}

		static string GetStringValue(XElement element, string name)
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
