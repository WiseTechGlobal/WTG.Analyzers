using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class SemanticModelExtensionsTest
	{
		[TestCase("0", ExpectedResult = true)]
		[TestCase("0u", ExpectedResult = true)]
		[TestCase("0l", ExpectedResult = true)]
		[TestCase("0L", ExpectedResult = true)]
		[TestCase("0ul", ExpectedResult = true)]
		[TestCase("0UL", ExpectedResult = true)]
		[TestCase("00", ExpectedResult = true)]
		[TestCase("0x0", ExpectedResult = true)]
		[TestCase("0b0000___0000_000", ExpectedResult = true)]
		[TestCase("-0", ExpectedResult = true)]
		[TestCase("+0", ExpectedResult = true)]
		[TestCase("(0)", ExpectedResult = true)]
		[TestCase("(byte)0", ExpectedResult = true)]
		[TestCase("(short)0", ExpectedResult = true)]
		[TestCase("'\\0'", ExpectedResult = true)]
		[TestCase("1", ExpectedResult = false)]
		[TestCase("+42", ExpectedResult = false)]
		[TestCase("-99", ExpectedResult = false)]
		[TestCase("(1)", ExpectedResult = false)]
		[TestCase("0x001", ExpectedResult = false)]
		[TestCase("0b001", ExpectedResult = false)]
		[TestCase("1u", ExpectedResult = false)]
		[TestCase("1l", ExpectedResult = false)]
		[TestCase("1U", ExpectedResult = false)]
		[TestCase("1L", ExpectedResult = false)]
		[TestCase("1UL", ExpectedResult = false)]
		[TestCase("(byte)1", ExpectedResult = false)]
		[TestCase("(short)1", ExpectedResult = false)]
		[TestCase("'\\1'", ExpectedResult = false)]
		[TestCase("()", ExpectedResult = false)]
		public async Task<bool> IsConstantZero(string value)
		{
			var source = $@"static class Foo {{ public static object Value => {value}; }} ";
			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			var semanticModel = compilation.GetSemanticModel(tree);

			var root = await tree.GetRootAsync().ConfigureAwait(false);
			var exp = root.DescendantNodes().OfType<ArrowExpressionClauseSyntax>().FirstOrDefault().Expression;

			return semanticModel.IsConstantZero(exp, default);
		}
	}
}
