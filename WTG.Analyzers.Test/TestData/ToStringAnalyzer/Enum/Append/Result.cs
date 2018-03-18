using System;

public static class Foo
{
	public static string Method0() => "[" + nameof(Items.Value0) + "]";
}

public enum Items
{
	Value0 = 0,
	Value1 = 1,
	Value2 = 2,
}
