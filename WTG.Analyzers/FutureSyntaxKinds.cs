using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	static class FutureSyntaxKinds
    {
		const int DefaultLiteralExpression = 8755;

		public static bool IsDefaultLiteralExpression(SyntaxKind kind) => kind == (SyntaxKind)DefaultLiteralExpression;
	}
}
