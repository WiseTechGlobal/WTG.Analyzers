using System.Diagnostics.Contracts;

public static class Bob
{
	[Pure]
	public static bool Method(int value) => value == 42;
}
