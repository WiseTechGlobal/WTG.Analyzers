using System;

public static class Foo
{
	public static string Method() => Flags.Value1.ToString(42);
}

[Flags]
public enum Flags
{
	None = 0,
	Value1 = 1,
	Value2 = 2,
}
