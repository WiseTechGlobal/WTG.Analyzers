using System;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class TriviaSyntaxFactoryTest
	{
		[Test]
		public void PlatformNewLineTrivia()
		{
			var trivia = TriviaSyntaxFactory.PlatformNewLineTrivia;

			Assert.That(trivia.Kind(), Is.EqualTo(SyntaxKind.EndOfLineTrivia));
			Assert.That(trivia.ToFullString(), Is.EqualTo(Environment.NewLine));
		}
	}
}
