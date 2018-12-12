using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class DirectiveHelperTest
	{
		[TestCase("A", "H", ExpectedResult = false)]
		[TestCase("A", "D", ExpectedResult = false)]
		[TestCase("D", "E", ExpectedResult = false)]
		[TestCase("F", "G", ExpectedResult = false)]
		[TestCase("A", "B", ExpectedResult = true)]
		[TestCase("A", "C", ExpectedResult = true)]
		[TestCase("A", "F", ExpectedResult = true)]
		[TestCase("C", "F", ExpectedResult = true)]
		[TestCase("C", "D", ExpectedResult = true)]
		[TestCase("C", "H", ExpectedResult = true)]
		public bool BoundingTriviaSplitsNodes(string start, string end)
		{
			return DirectiveHelper.BoundingTriviaSplitsNodes(trivia[start], trivia[end]);
		}

		#region Implementation

		[OneTimeSetUp]
		protected async Task Setup()
		{
			const string text =
@"
using System;

namespace NS
{
	class C
	{
		/*A*/ public /*B*/
		static void Method1()
		{
			/*C*/
		}

		/*D*/

		/*E*/

		public static void Method2()
		{
			/*F*/;/*G*/
		}

		/*H*/
	}
}
";
			tree = SyntaxFactory.ParseSyntaxTree(text);
			var root = await tree.GetRootAsync().ConfigureAwait(false);

			trivia = Enumerable.ToDictionary(
				from trivia in root.DescendantTrivia()
				where trivia.Kind() == SyntaxKind.MultiLineCommentTrivia
				let raw = trivia.ToString()
				let name = raw.Substring(2, raw.Length - 4)
				select new { name, trivia },
				x => x.name,
				x => x.trivia);
		}

		SyntaxTree tree;
		IReadOnlyDictionary<string, SyntaxTrivia> trivia;

		#endregion
	}
}
