using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers
{
	static class NonNullExtensions
	{
		public static Task<SyntaxTree> RequireSyntaxTreeAsync(this Document document, CancellationToken cancellation)
		{
			if (!document.SupportsSyntaxTree)
			{
				return Task.FromException<SyntaxTree>(new NotSupportedException("Document doesn't support syntax trees."));
			}

			return document.GetSyntaxTreeAsync(cancellation)!;
		}

		public static Task<SyntaxNode> RequireSyntaxRootAsync(this Document document, CancellationToken cancellationToken)
		{
			if (!document.SupportsSyntaxTree)
			{
				return Task.FromException<SyntaxNode>(new NotSupportedException("Document doesn't support syntax trees."));
			}

			return document.GetSyntaxRootAsync(cancellationToken)!;
		}

		public static Task<SemanticModel> RequireSemanticModelAsync(this Document document, CancellationToken cancellationToken)
		{
			if (!document.SupportsSemanticModel)
			{
				return Task.FromException<SemanticModel>(new NotSupportedException("Document doesn't support a semantic model."));
			}

			return document.GetSemanticModelAsync(cancellationToken)!;
		}
	}
}
