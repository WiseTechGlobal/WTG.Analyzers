using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class FutureSyntaxKindsTest
	{
		[Test]
		public void AllFutureSyntaxKindsAreKnownAndValid()
		{
			// Note: We are asserting that the specified syntax kinds exist in the version of roslyn that the tests are using,
			// which may be later than the version that the production code is referencing.
			Assert.Multiple(() =>
			{
				var fields = typeof(FutureSyntaxKinds).GetFields(BindingFlags.Public | BindingFlags.Static);

				foreach (var field in fields)
				{
					if (!field.IsLiteral || field.FieldType != typeof(SyntaxKind))
					{
						continue;
					}

					if (Enum.TryParse<SyntaxKind>(field.Name, ignoreCase: false, out var expectedValue))
					{
						Assert.That(field.GetValue(null), Is.EqualTo(expectedValue));
					}
					else
					{
						Assert.Fail($"'{field.Name}' is not a recognised syntax kind.");
					}
				}
			});
		}
	}
}
