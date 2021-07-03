using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class InterpolationInfoTest
	{
		[TestCase("$\"FooBar\"", "FooBar", TestName = "{m}({0})")]
		[TestCase("$\"Foo{{Bar}}\"", "Foo{{Bar}}", TestName = "{m}({0})")]
		[TestCase("$\"Prefix{Foo}Infix{Bar}Postfix\"", "Prefix{0}Infix{1}Postfix", "Foo", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"{Bar}\"", "{0}", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,8}B\"", "A{0,8}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,-12}B\"", "A{0,-12}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,+12}B\"", "A{0,12}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,12-4}B\"", "A{0,8}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,4+X}B\"", "A{0,6}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar:Baz}B\"", "A{0:Baz}B", "Bar", TestName = "{m}({0})")]
		[TestCase("$\"A{Bar,8:Baz}B\"", "A{0,8:Baz}B", "Bar", TestName = "{m}({0})")]
		public async Task Extract(string source, string format, params string[] identifiers)
		{
			var (model, expression) = await SetupSemanticModel(source).ConfigureAwait(false);
			var info = InterpolationInfo.Extract(model, expression, CancellationToken.None);
			var identifierNames = info.Expressions.Cast<IdentifierNameSyntax>().Select(x => x.Identifier.Text).ToArray();
			Assert.That(info.Format, Is.EqualTo(format));
			Assert.That(identifierNames, Is.EqualTo(identifiers));
			Assert.That(() => string.Format(CultureInfo.InvariantCulture, format, identifierNames), Throws.Nothing);
		}

		[Test]
		public async Task ExtractNonConstantAlignment()
		{
			var (model, expression) = await SetupSemanticModel("$\"{foo,4+Y}\"").ConfigureAwait(false);
			var info = InterpolationInfo.Extract(model, expression, CancellationToken.None);
			var identifierNames = info.Expressions.Cast<IdentifierNameSyntax>().Select(x => x.Identifier.Text).ToArray();

			Assert.That(info.Format, Is.EqualTo("{0,4+Y}"));
			Assert.That(identifierNames, Is.EqualTo(new[] { "foo" }));
		}

		static async Task<(SemanticModel Model, InterpolatedStringExpressionSyntax Expression)> SetupSemanticModel(string interpolatedString)
		{
			var prefix = @"static class Host
{
	public static string Poke() => ";

			var suffix = @";
	const int X = 2;
}
";
			var source = prefix + interpolatedString + suffix;
			var document = ModelUtils.CreateDocument(source);
			var model = await document.GetSemanticModelAsync().ConfigureAwait(false);
			var tree = await document.GetSyntaxRootAsync().ConfigureAwait(false);

			var expression = (InterpolatedStringExpressionSyntax)tree.FindNode(TextSpan.FromBounds(prefix.Length, source.Length - suffix.Length));

			return (model, expression);
		}
	}
}
