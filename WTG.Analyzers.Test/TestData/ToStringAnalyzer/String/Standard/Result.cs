using System;

public static class Foo
{
	public static string Method(string value) => value;
	public static string Method(string value, IFormatProvider formatProvider) => value;

	public static string Append(string value) => "[" + value + "]";
}
