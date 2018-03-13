using System;
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
			var ruleSet = XElement.Load(@"..\..\WTG.Analyzers\build\WarnAll.ruleset")
				.Element("Rules")
				.Elements("Rule")
				.ToDictionary(
					x => x.Attribute("Id").Value,
					x => (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), x.Attribute("Action").Value));

			var descriptors = typeof(Rules).GetFields()
				.Where(x => x.IsStatic && x.IsInitOnly && typeof(DiagnosticDescriptor).IsAssignableFrom(x.FieldType))
				.Select(x => (DiagnosticDescriptor)x.GetValue(null));

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
			});
		}
	}
}
