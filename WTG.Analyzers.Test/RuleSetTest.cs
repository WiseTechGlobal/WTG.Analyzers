using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace WTG.Analyzers.Test
{
	[TestFixture]
	public class RuleSetTest
	{
		[Test]
		public void WarnAll()
		{
			var rulesetPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "WTG.Analyzers", "build", "WarnAll.editorconfig"));

			var prefix = "dotnet_diagnostic.";
			var delimiter = new[] { '=' };

			var ruleSet = File.ReadAllLines(rulesetPath)
				.Select(line => line.Trim())
				.Where(line => line.StartsWith(prefix, StringComparison.Ordinal))
				.Select(line => line.Split(delimiter, count: 2))
				.ToDictionary(
					parts => parts[0].Substring(prefix.Length, length: parts[0].IndexOf(".", startIndex: prefix.Length, StringComparison.Ordinal) - prefix.Length),
					parts => (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), parts[1].Trim(), ignoreCase: true));

			var descriptors = typeof(Rules).GetFields()
				.Where(x => x.IsStatic && x.IsInitOnly && typeof(DiagnosticDescriptor).IsAssignableFrom(x.FieldType))
				.Select(x => (DiagnosticDescriptor)x.GetValue(null))
				.ToArray();

			Assert.Multiple(() =>
			{
				foreach (var descriptor in descriptors)
				{
					if (!ruleSet.TryGetValue(descriptor.Id, out var severity))
					{
						severity = descriptor.DefaultSeverity;
					}

					Assert.That(severity, Is.EqualTo(DiagnosticSeverity.Warning), descriptor.Id);
				}

				var definedRules = new HashSet<string>(descriptors.Select(x => x.Id));

				foreach (var rule in ruleSet)
				{
					if (!definedRules.Contains(rule.Key))
					{
						Assert.Fail($"Referenced the undefined rule '{rule.Key}.");
					}
				}
			});
		}
	}
}
