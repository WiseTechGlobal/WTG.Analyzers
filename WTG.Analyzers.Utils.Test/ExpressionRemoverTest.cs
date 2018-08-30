using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class ExpressionRemoverTest
	{
		[TestCase("__TRUE__ && exp", ExpectedResult = "exp")]
		[TestCase("__FALSE__ && exp", ExpectedResult = "false")]
		[TestCase("exp && __TRUE__", ExpectedResult = "exp")]
		[TestCase("exp && __FALSE__", ExpectedResult = "exp && false")] // need to keep exp incase it has side effects.
		[TestCase("__TRUE__ || exp", ExpectedResult = "true")]
		[TestCase("__FALSE__ || exp", ExpectedResult = "exp")]
		[TestCase("exp || __TRUE__", ExpectedResult = "exp || true")] // need to keep exp incase it has side effects.
		[TestCase("exp || __FALSE__", ExpectedResult = "exp")]
		[TestCase("(__TRUE__)", ExpectedResult = "true")]
		[TestCase("!__TRUE__", ExpectedResult = "false")]
		[TestCase("__TRUE__ ? A : B", ExpectedResult = "A")]
		[TestCase("__FALSE__ ? A : B", ExpectedResult = "B")]
		[TestCase("exp ? __TRUE__ : B", ExpectedResult = "exp || B")]
		[TestCase("exp ? __FALSE__ : B", ExpectedResult = "!exp && B")]
		[TestCase("exp ? A : __TRUE__", ExpectedResult = "!exp || A")]
		[TestCase("exp ? A : __FALSE__", ExpectedResult = "exp && A")]
		[TestCase("exp ? __TRUE__ : __FALSE__", ExpectedResult = "exp")]
		[TestCase("exp ? __FALSE__ : __TRUE__ ", ExpectedResult = "!exp")]
		[TestCase("exp ? __TRUE__ : __TRUE__ ", ExpectedResult = "exp || true")] // need to keep exp incase it has side effects.
		[TestCase("exp ? __FALSE__ : __FALSE__ ", ExpectedResult = "exp && false")] // need to keep exp incase it has side effects.
		public string ReplaceWithConstantBool_Expression(string expressionText)
		{
			return ApplyToAllMagicTokens(SyntaxFactory.ParseExpression(expressionText));
		}

		[TestCase("if (__TRUE__) A();", ExpectedResult = "A();")]
		[TestCase("if (__FALSE__) A();", ExpectedResult = ";")]
		[TestCase("{ X(); if (__FALSE__) A(); X(); }", ExpectedResult = "{ X(); X(); }")]
		[TestCase("if (__TRUE__) A(); else B();", ExpectedResult = "A();")]
		[TestCase("if (__FALSE__) A(); else B();", ExpectedResult = "B();")]
		[TestCase("if (exp) A(); else if (__FALSE__) B();", ExpectedResult = "if (exp) A();")]
		[TestCase("if (exp) A(); else if (__TRUE__) B();", ExpectedResult = "if (exp) A(); else B();")]
		[TestCase("if (exp) A(); else { if (__FALSE__) B(); }", ExpectedResult = "if (exp) A();")]
		[TestCase("while (__FALSE__) A();", ExpectedResult = ";")]
		[TestCase("{ X(); while (__FALSE__) A(); X(); }", ExpectedResult = "{ X(); X(); }")]
		[TestCase("while (__TRUE__) A();", ExpectedResult = "while (true) A();")]
		[TestCase("do A(); while (__FALSE__);", ExpectedResult = "A();")]
		[TestCase("do A(); while (__TRUE__);", ExpectedResult = "do A(); while (true);")]
		public string ReplaceWithConstantBool_Statement(string statementText)
		{
			return ApplyToAllMagicTokens(SyntaxFactory.ParseStatement(statementText));
		}

		[Test]
		public void InlineIfFormatting()
		{
			const string Source =
@"{
	X();

	// comment 1
	if (__TRUE__)
	{
		// comment 2
		A();

		B();
		// comment 3
	}
	// comment 4

	X();
}";

			const string Expected =
@"{
	X();

	// comment 1

	// comment 2
	A();

	B();
	// comment 3

	// comment 4

	X();
}";

			var actual = ApplyToAllMagicTokens(SyntaxFactory.ParseStatement(Source), true);
			Assert.That(actual, Is.EqualTo(Expected));
		}

		[Test]
		public void InlineElseFormatting()
		{
			const string Source =
@"{
	X();

	// comment 1
	if (__FALSE__)
	{
		// comment 2
		A();

		B();
		// comment 3
	}
	else
	{
		// comment 4
		C();

		D();
		// comment 5
	}
	// comment 6

	X();
}";

			const string Expected =
@"{
	X();

	// comment 1

	// comment 4
	C();

	D();
	// comment 5

	// comment 6

	X();
}";

			var actual = ApplyToAllMagicTokens(SyntaxFactory.ParseStatement(Source), true);
			Assert.That(actual, Is.EqualTo(Expected));
		}

		[Test]
		public void InlineElseIfFormatting()
		{
			const string Source =
@"{
	X();

	if (__FALSE__)
	{
		A();
	}
	else if (expression)
	{
		B();
	}

	X();
}";

			const string Expected =
@"{
	X();

	if (expression)
	{
		B();
	}

	X();
}";

			var actual = ApplyToAllMagicTokens(SyntaxFactory.ParseStatement(Source), true);
			Assert.That(actual, Is.EqualTo(Expected));
		}

		[Test]
		public void InlineDoFormatting()
		{
			const string Source =
@"{
	X();

	// comment 1
	do
	{
		// comment 2
		A();

		B();
		// comment 3
	}
	while (__FALSE__);
	// comment 4

	X();
}";

			const string Expected =
@"{
	X();

	// comment 1

	// comment 2
	A();

	B();
	// comment 3

	// comment 4

	X();
}";

			var actual = ApplyToAllMagicTokens(SyntaxFactory.ParseStatement(Source), true);
			Assert.That(actual, Is.EqualTo(Expected));
		}

		#region Implementation

		static string ApplyToAllMagicTokens(SyntaxNode expressionSyntax, bool reformat = false)
		{
			var node = ExpressionRemover.ReplaceWithConstantBool(expressionSyntax, FindMagicTokens(expressionSyntax));

			if (reformat)
			{
				using (var workspace = new AdhocWorkspace())
				{
					node = Formatter.Format(node, Formatter.Annotation, workspace);
				}
			}

			return node.ToString();
		}

		static ImmutableDictionary<SyntaxNode, bool> FindMagicTokens(SyntaxNode node)
		{
			var builder = ImmutableDictionary.CreateBuilder<SyntaxNode, bool>();

			foreach (var tmp in node.DescendantNodesAndSelf())
			{
				if (tmp.IsKind(SyntaxKind.IdentifierName))
				{
					var identifier = (IdentifierNameSyntax)tmp;

					switch (identifier.Identifier.ValueText)
					{
						case "__TRUE__":
							builder.Add(identifier, true);
							break;

						case "__FALSE__":
							builder.Add(identifier, false);
							break;
					}
				}
			}

			return builder.ToImmutable();
		}

		#endregion
	}
}
