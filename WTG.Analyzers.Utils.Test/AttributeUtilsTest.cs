using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class AttributeUtilsTest
	{
		[TestCase(0, ExpectedResult = "1")]
		[TestCase(1, ExpectedResult = "2")]
		[TestCase(2, ExpectedResult = null)]
		public string ValueByIndex(int index)
		{
			var value = (LiteralExpressionSyntax)attributes[0].GetArgumentValue(index);
			return value == null ? null : (string)value.Token.Value;
		}

		[TestCase("Prop1", ExpectedResult = "A")]
		[TestCase("Prop2", ExpectedResult = "B")]
		[TestCase("Prop3", ExpectedResult = null)]
		public string ValueByName(string name)
		{
			var value = (LiteralExpressionSyntax)attributes[0].GetPropertyValue(name);
			return value == null ? null : (string)value.Token.Value;
		}

		[TestCase(0, ExpectedResult = "(1, 0)-(1, 53)")]
		[TestCase(1, ExpectedResult = "(3, 1)-(3, 42)")]
		[TestCase(2, ExpectedResult = "(4, 1)-(4, 42)")]
		public string GetLocation(int index)
		{
			var location = AttributeUtils.GetLocation(attributes[index]);
			var span = location.GetMappedLineSpan();

			return $"({span.StartLinePosition.Line}, {span.StartLinePosition.Character})-({span.EndLinePosition.Line}, {span.EndLinePosition.Character})";
		}

		#region Implementation

		[OneTimeSetUp]
		protected async Task Setup()
		{
			var source =
@"using System;
[assembly: Magic(""1"", ""2"", Prop1 = ""A"", Prop2 = ""B"")]
[assembly:
	Magic(""3"", ""4"", Prop1 = ""C"", Prop2 = ""D""),
	Magic(""5"", ""6"", Prop1 = ""E"", Prop2 = ""F"")
]

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
			var lists = tree.GetCompilationUnitRoot().AttributeLists;

			attributes = new[]
			{
				lists[0].Attributes.Single(),
				lists[1].Attributes[0],
				lists[1].Attributes[1],
			};
		}

		AttributeSyntax[] attributes;

		#endregion
	}
}
