using System;

public static class Foo
{
	// These are actually non-deterministic, but give the developer the benifit of the doubt.
	public static string Method1() => Items.Value1a.ToString();
	public static string Method2() => Items.Value1b.ToString();
}

public enum Items
{
	Value0 = 0,
	Value1a = 1,
	Value1b = 1,
}
