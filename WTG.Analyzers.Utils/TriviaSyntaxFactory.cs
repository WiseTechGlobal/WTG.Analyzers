using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers.Utils
{
    public static class TriviaSyntaxFactory
    {
        public static SyntaxTrivia PlatformNewLineTrivia { get; } = Environment.NewLine switch
        {
            "\r\n" => SyntaxFactory.CarriageReturnLineFeed,
            "\r" => SyntaxFactory.CarriageReturn,
            "\n" => SyntaxFactory.LineFeed,
            _ => throw new PlatformNotSupportedException(),
        };
    }
}
