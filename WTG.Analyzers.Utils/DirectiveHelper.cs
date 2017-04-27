using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	public static class DirectiveHelper
	{
		public static bool BoundingTriviaSplitsNodes(SyntaxTrivia startTrivia, SyntaxTrivia endTrivia)
		{
			if (startTrivia == endTrivia)
			{
				return false;
			}

			var startToken = startTrivia.Token;
			var endToken = endTrivia.Token;

			if (startToken == endToken && (endTrivia.SpanStart < startToken.SpanStart || startTrivia.SpanStart > endToken.SpanStart))
			{
				// If both trivia are on the same side of the same token, then that indicates that no node is between them.
				return false;
			}

			if (startTrivia.SpanStart > startToken.SpanStart)
			{
				startToken = startToken.GetNextToken();
			}

			if (endTrivia.SpanStart < endToken.SpanStart)
			{
				endToken = endToken.GetPreviousToken();
			}

			var startNode = startToken.Parent;
			var endNode = endToken.Parent;

			ElevateToCommonScope(ref startNode, ref endNode);

			return startNode.GetFirstToken() != startToken || endNode.GetLastToken() != endToken;
		}

		static void ElevateToCommonScope(ref SyntaxNode node1, ref SyntaxNode node2)
		{
			if (node1 == node2 || node1.Parent == node2.Parent)
			{
				return;
			}

			for (var tmp = node1; tmp != null; tmp = tmp.Parent)
			{
				if (tmp.Span.Contains(node2.SpanStart))
				{
					break;
				}

				node1 = tmp;
			}

			for (var tmp = node2; tmp != null; tmp = tmp.Parent)
			{
				if (tmp.Span.Contains(node1.SpanStart))
				{
					break;
				}

				node2 = tmp;
			}
		}
	}
}
