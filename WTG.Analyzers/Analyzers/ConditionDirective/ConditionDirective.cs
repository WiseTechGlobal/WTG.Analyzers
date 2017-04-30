using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	sealed class ConditionDirective
	{
		public static ImmutableArray<ConditionDirective> Extract(SyntaxNode node)
		{
			var stack = new Stack<IfInfo>();
			var builder = ImmutableArray.CreateBuilder<ConditionDirective>();

			foreach (var trivia in node.DescendantTrivia())
			{
				switch (trivia.Kind())
				{
					case SyntaxKind.IfDirectiveTrivia:
						stack.Push(new IfInfo(trivia));
						break;

					case SyntaxKind.ElifDirectiveTrivia:
						if (stack.Count > 0)
						{
							var info = stack.Peek();

							if (info.Else.IsKind(SyntaxKind.None))
							{
								info.ElIf.Add(trivia);
							}
						}
						break;

					case SyntaxKind.ElseDirectiveTrivia:
						if (stack.Count > 0)
						{
							var info = stack.Peek();

							if (info.Else.IsKind(SyntaxKind.None))
							{
								info.Else = trivia;
							}
						}
						break;

					case SyntaxKind.EndIfDirectiveTrivia:
						if (stack.Count > 0)
						{
							var info = stack.Pop();

							builder.Add(
								new ConditionDirective(
									info.If,
									info.ElIf.ToImmutable(),
									info.Else,
									trivia));
						}
						break;
				}
			}

			return builder.ToImmutable();
		}

		ConditionDirective(SyntaxTrivia ifDirective, ImmutableArray<SyntaxTrivia> elseIfDirectives, SyntaxTrivia elseDirective, SyntaxTrivia endifDirective)
		{
			If = ifDirective;
			ElseIf = elseIfDirectives;
			Else = elseDirective;
			End = endifDirective;
		}

		public SyntaxTrivia If { get; }
		public ImmutableArray<SyntaxTrivia> ElseIf { get; }
		public SyntaxTrivia Else { get; }
		public SyntaxTrivia End { get; }

		sealed class IfInfo
		{
			public IfInfo(SyntaxTrivia @if)
			{
				If = @if;
				ElIf = ImmutableArray.CreateBuilder<SyntaxTrivia>();
			}

			public SyntaxTrivia If { get; }
			public ImmutableArray<SyntaxTrivia>.Builder ElIf { get; }
			public SyntaxTrivia Else { get; set; }
		}
	}
}
