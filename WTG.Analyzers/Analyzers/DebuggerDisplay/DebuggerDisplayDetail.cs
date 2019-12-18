using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class DebuggerDisplayDetail
	{
		DebuggerDisplayDetail(ImmutableArray<ExpressionSyntax> values, bool hasError = false)
		{
			Values = values;
			HasError = hasError;
		}

		public ImmutableArray<ExpressionSyntax> Values { get; }
		public bool HasError { get; }

		public static DebuggerDisplayDetail Parse(string format)
		{
			if (string.IsNullOrEmpty(format))
			{
				return new DebuggerDisplayDetail(ImmutableArray<ExpressionSyntax>.Empty);
			}

			var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
			var i = 0;

			while (i < format.Length)
			{
				var c = format[i];

				if (c == '{')
				{
					if (++i >= format.Length)
					{
						return CreateFault(builder);
					}
					else if (format[i] == '{')
					{
						goto next;
					}

					var expression = SyntaxFactory.ParseExpression(format, i, consumeFullText: false);
					i += NextChar(expression);
					builder.Add(expression);

					while (i < format.Length && char.IsWhiteSpace(format[i]))
					{
						i++;
					}

					if (i >= format.Length)
					{
						return CreateFault(builder);
					}
					else if (format[i] == '}')
					{
						goto next;
					}
					else if (i + 3 >= format.Length || string.Compare(format, i, ",nq}", 0, 4, StringComparison.Ordinal) != 0)
					{
						return CreateFault(builder);
					}

					i += 4;
				}
				else if (c == '}')
				{
					if (++i >= format.Length || format[i] != '}')
					{
						return CreateFault(builder);
					}
				}

next:
				i++;
			}

			return new DebuggerDisplayDetail(builder.ToImmutable());

			static DebuggerDisplayDetail CreateFault(ImmutableArray<ExpressionSyntax>.Builder b) => new DebuggerDisplayDetail(b.ToImmutable(), true);
		}

		static int NextChar(ExpressionSyntax expression) => expression.GetLocation().SourceSpan.End;
	}
}
