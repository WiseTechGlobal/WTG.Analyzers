using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using WTG.Analyzers;

static class NRT
{
	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool value, [CallerArgumentExpression("value")] string? message = null)
	{
#if DEBUG
		if (!value)
		{
			Fail(message ?? "Assertion Failed.");
		}
#endif
	}

#if DEBUG
	[DoesNotReturn]
	static void Fail(string message) => throw new AssertionFailedException(message);
#endif
}
