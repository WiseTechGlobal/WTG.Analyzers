using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace WTG.Analyzers.Test
{
	[TestFixture]
	class RuleSetTest
	{
		[Test]
		public void WarnAll()
		{
			var rulesetPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "WTG.Analyzers", "build", "WarnAll.ruleset"));

			var ruleSet = XElement.Load(rulesetPath)
				.Element("Rules")
				.Elements("Rule")
				.ToDictionary(
					x => x.Attribute("Id").Value,
					x => (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), x.Attribute("Action").Value));

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
