using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using WTG.Analyzers.Test.Helpers;

namespace WTG.Analyzers.Test
{
	internal sealed class SampleDataSet
	{
		SampleDataSet(string source, string result, DiagnosticResult[] diagnostics)
		{
			Source = source;
			Result = result;
			Diagnostics = ImmutableArray.Create(diagnostics);
		}

		public string Source { get; }
		public string Result { get; }
		public ImmutableArray<DiagnosticResult> Diagnostics { get; }

		public static IEnumerable<string> GetSampleNames(string analyzerName)
		{
			var prefix = TestDataPrefix + analyzerName + ".";

			return Enumerable.Distinct(
				from name in typeof(SampleDataSet).Assembly.GetManifestResourceNames()
				where name.StartsWith(prefix)
				let index = name.IndexOf('.', prefix.Length)
				where index >= 0
				select name.Substring(prefix.Length, index - prefix.Length));
		}

		public static SampleDataSet GetSampleData(string analyzerName, string sampleName)
		{
			var prefix = TestDataPrefix + analyzerName + "." + sampleName + ".";

			return new SampleDataSet(
				LoadResource(prefix + "Source.cs"),
				LoadResource(prefix + "Result.cs"),
				LoadResults(prefix + "Diagnostics.xml"));
		}

		static string LoadResource(string name)
		{
			using (var stream = typeof(SampleDataSet).Assembly.GetManifestResourceStream(name))
			{
				Assert.That(stream, Is.Not.Null);

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		static DiagnosticResult[] LoadResults(string name)
		{
			var text = LoadResource(name);
			var root = XElement.Parse(text);
			return root.Descendants("diagnostic").Select(LoadResult).ToArray();
		}

		static DiagnosticResult LoadResult(XElement element)
		{
			return new DiagnosticResult(
				GetStringValue(element, "id"),
				GetEnumValue<DiagnosticSeverity>(element, "severity"),
				GetStringValue(element, "message"),
				element.Elements("location").Select(LoadLocation).ToArray());
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

			var startLine = int.Parse(match.Groups["startLine"].Value);
			var startColumn = int.Parse(match.Groups["startColumn"].Value);

			var endLineGroup = match.Groups["endLine"];
			var endColumnGroup = match.Groups["endColumn"];

			var endLine = endLineGroup.Success ? int.Parse(endLineGroup.Value) : startLine;
			var endColumn = endColumnGroup.Success ? int.Parse(endColumnGroup.Value) : startColumn;

			return new DiagnosticResultLocation(
				match.Groups["path"].Value,
				startLine,
				startColumn,
				endLine,
				endColumn);
		}

		const string TestDataPrefix = "WTG.Analyzers.Test.TestData.";
	}
}
