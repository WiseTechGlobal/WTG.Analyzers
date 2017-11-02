using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class FirstLinqMethod : LinqMethod
	{
		public override bool IsMatch(IMethodSymbol method) => IsEnumerableLinqMethod(method, nameof(Enumerable.First), 1);
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
				return CreateIndexerDiagnostic(invoke, sourceType, nameof(Enumerable.First) + "()", isInAnExpression);
			}

			public override ExpressionSyntax ApplyFix(InvocationExpressionSyntax invoke)
			{
				var arguments = invoke.ArgumentList.Arguments;

				return ExpressionSyntaxFactory.CreateElementAccessExpression(
					arguments.Count > 0
						? arguments[0].Expression
						: LinqUtils.GetInstanceExpression(invoke),
					ExpressionSyntaxFactory.CreateLiteral(0));
			}
		}
	}
}
