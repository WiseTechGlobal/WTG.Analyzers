using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class ElementAtLinqMethod : LinqMethod
	{
		public override bool IsMatch(IMethodSymbol method) => IsEnumerableLinqMethod(method, nameof(Enumerable.ElementAt), 2);
		public override LinqResolution GetResolution(ITypeSymbol sourceType)
		{
			if (sourceType.Kind == SymbolKind.ArrayType || HasIndexer(sourceType))
			{
				return Resolution.Instance;
			}

			return null;
		}

		sealed class Resolution : LinqResolution
		{
			public static Resolution Instance { get; } = new Resolution();

			Resolution()
			{
			}

			public override Diagnostic CreateDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, bool isInAnExpression)
			{
				return CreateIndexerDiagnostic(invoke, sourceType, nameof(Enumerable.ElementAt) + "(int)", isInAnExpression);
			}

			public override ExpressionSyntax ApplyFix(InvocationExpressionSyntax invoke)
			{
				var arguments = invoke.ArgumentList.Arguments;

				if (arguments.Count > 1)
				{
					return ExpressionSyntaxFactory.CreateElementAccessExpression(
						arguments[0].Expression,
						arguments[1].Expression);
				}
				else
				{
					return ExpressionSyntaxFactory.CreateElementAccessExpression(
						LinqUtils.GetInstanceExpression(invoke),
						arguments[0].Expression);
				}
			}
		}
	}
}
