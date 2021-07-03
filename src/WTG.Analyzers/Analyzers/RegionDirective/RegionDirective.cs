using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	sealed class RegionDirective
	{
		public static ImmutableArray<RegionDirective> Extract(SyntaxNode node)
		{
			var stack = new Stack<SyntaxTrivia>();
			var builder = ImmutableArray.CreateBuilder<RegionDirective>();

			foreach (var trivia in node.DescendantTrivia())
			{
				switch (trivia.Kind())
				{
					case SyntaxKind.RegionDirectiveTrivia:
						stack.Push(trivia);
						break;

					case SyntaxKind.EndRegionDirectiveTrivia:
						if (stack.Count > 0)
						{
							var start = stack.Pop();
							builder.Add(new RegionDirective(start, trivia, stack.Count));
						}
						break;
				}
			}

			return builder.ToImmutable();
		}

		RegionDirective(SyntaxTrivia start, SyntaxTrivia end, int depth)
		{
			Start = start;
			End = end;
			Depth = depth;
		}

		public SyntaxTrivia Start { get; }
		public SyntaxTrivia End { get; }
		public int Depth { get; }
	}
}
