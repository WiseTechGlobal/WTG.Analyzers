using System;

public static class Foo
{
	public static string Method(string value) => value.ToString();
	public static string Method(string value, IFormatProvider formatProvider) => value.ToString(formatProvider);

	public static string Append(string value) => "[" + value.ToString() + "]";
}
