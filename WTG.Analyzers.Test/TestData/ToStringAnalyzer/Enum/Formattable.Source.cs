using System;

public static class Foo
{
	public static string Formattable(string format) => Items.Value0.ToString(format);
}

public enum Items
{
	Value0 = 0,
	Value1 = 1,
	Value2 = 2,
}
