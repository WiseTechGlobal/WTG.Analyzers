using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers.Utils
{
    public static class TriviaSyntaxFactory
    {
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Reading Environment.NewLine for formatting, not reading settings.")]
        public static SyntaxTrivia PlatformNewLineTrivia { get; } = Environment.NewLine switch
        {
            "\r\n" => SyntaxFactory.CarriageReturnLineFeed,
            "\r" => SyntaxFactory.CarriageReturn,
            "\n" => SyntaxFactory.LineFeed,
            _ => throw new PlatformNotSupportedException(),
        };
    }
}
