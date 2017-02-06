using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	internal class AttributeUtilsTest
	{
		[TestCase(0, ExpectedResult = "1")]
		[TestCase(1, ExpectedResult = "2")]
		[TestCase(2, ExpectedResult = null)]
		public string ValueByIndex(int index)
		{
			var value = (LiteralExpressionSyntax)attribute.GetArgumentValue(index);
			return value == null ? null : (string)value.Token.Value;
		}

		[TestCase("Prop1", ExpectedResult = "A")]
		[TestCase("Prop2", ExpectedResult = "B")]
		[TestCase("Prop3", ExpectedResult = null)]
		public string ValueByName(string name)
		{
			var value = (LiteralExpressionSyntax)attribute.GetPropertyValue(name);
			return value == null ? null : (string)value.Token.Value;
		}

		#region Implementation

		[OneTimeSetUp]
		protected async Task Setup()
		{
			string source =
@"using System;
[assembly: Magic(""1"", ""2"", Prop1 = ""A"", Prop2 = ""B"")]

sealed class MagicAttribute : Attribute
{
	public MagicAttribute(string arg1, string arg2, string arg3 = ""default"")
	{
	}

	public string Prop1 { get; set; }
	public string Prop2 { get; set; }
	public string Prop3 { get; set; }
}
";

			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			attribute = tree.GetCompilationUnitRoot().AttributeLists.Single().Attributes.Single();
		}

		AttributeSyntax attribute;

		#endregion
	}
}
