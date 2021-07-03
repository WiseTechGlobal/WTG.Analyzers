using System;

public static class Foo
{
	public static string Method()
	{
		return Flags.Value1.ToString();
	}
}

[Flags]
public enum Flags
{
	None = 0,
	Value1 = 1,
	Value2 = 2,
}
