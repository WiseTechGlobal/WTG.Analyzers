using System;

public static class Foo
{
	public static string Method0() => "[" + Items.Value0.ToString() + "]";
}

public enum Items
{
	Value0 = 0,
	Value1 = 1,
	Value2 = 2,
}
