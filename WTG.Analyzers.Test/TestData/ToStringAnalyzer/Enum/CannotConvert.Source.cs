using System;

public static class Foo
{
	public static string Method(Items value) => value.ToString();
	public static string Converted => ((Items)0).ToString();
}

public enum Items
{
	Value0 = 0,
	Value1 = 1,
	Value2 = 2,
}
