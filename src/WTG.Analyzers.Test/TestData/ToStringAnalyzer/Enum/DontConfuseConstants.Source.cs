using System;

public static class Foo
{
	public static string Method() => Magic.Value.ToString();
}

public static class Magic
{
	public const Flags Value = Flags.Value1;
}

[Flags]
public enum Flags
{
	None = 0,
	Value1 = 1,
	Value2 = 2,
}
