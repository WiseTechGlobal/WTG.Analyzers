using System;

public static class Foo
{
	// These are actually non-deterministic, but give the developer the benifit of the doubt.
	public static string Method1() => nameof(Items.Value1a);
	public static string Method2() => nameof(Items.Value1b);
}

public enum Items
{
	Value0 = 0,
	Value1a = 1,
	Value1b = 1,
}
