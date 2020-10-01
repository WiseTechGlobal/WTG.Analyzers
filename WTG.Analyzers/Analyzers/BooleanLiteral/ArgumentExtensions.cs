using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class ArgumentExtensions
	{
		public static IParameterSymbol? TryFindCorrespondingParameterSymbol(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			var methodInvocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
			if (methodInvocation is null)
			{
				return null;
			}

			var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(methodInvocation, cancellationToken).Symbol;
			if (methodSymbol is null)
			{
				return null;
			}

			var argumentList = (ArgumentListSyntax)argument.Parent;
			var index = argumentList.FindIndexOfArgument(argument);
			var argumentSymbol = methodSymbol.Parameters[index];
			return argumentSymbol;
		}

		public static int FindIndexOfArgument(this ArgumentListSyntax haystack, ArgumentSyntax needle)
		{
			Debug.Assert(needle.Parent == haystack, "Argument should be a child of the ArgumentList.");

			var i = 0;
			var enumerator = haystack.Arguments.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == needle)
				{
					return i;
				}

				i++;
			}

			throw new InvalidOperationException("Failed to find Argument in its parent ArgumentList.");
		}
	}
}
