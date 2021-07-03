using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class CountLinqMethod : LinqMethod
	{
		public override bool IsMatch(IMethodSymbol method) => IsEnumerableLinqMethod(method, nameof(Enumerable.Count), 1);
		public override LinqResolution? GetResolution(ITypeSymbol sourceType)
		{
			if (sourceType.Kind == SymbolKind.ArrayType)
			{
				return Resolution.UseLength;
			}
			else if (HasCountProperty(sourceType))
			{
				return Resolution.UseCount;
			}
			else if (HasLengthProperty(sourceType))
			{
				return Resolution.UseLength;
			}

			return null;
		}

		sealed class Resolution : LinqResolution
		{
			public static Resolution UseLength { get; } = new Resolution("Length");
			public static Resolution UseCount { get; } = new Resolution("Count");

			Resolution(string name)
			{
				this.name = name;
			}

			public override Diagnostic CreateDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, bool isInAnExpression)
			{
				return CreatePropertyDiagnostic(invoke, sourceType, nameof(Enumerable.Count) + "()", name, isInAnExpression);
			}

			public override ExpressionSyntax ApplyFix(InvocationExpressionSyntax invoke)
			{
				return CreatePropertyAccessExpression(invoke, name);
			}

			readonly string name;
		}
	}
}
