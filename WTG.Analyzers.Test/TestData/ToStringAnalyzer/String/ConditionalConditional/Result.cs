using System;

public static class Foo
{
	public static int? Method(string value) => value?.Length;
	public static int? Method(string value, IFormatProvider formatProvider) => value?.Length;

	public static string Append(string value) => "[" + value?.Length + "]";
}
