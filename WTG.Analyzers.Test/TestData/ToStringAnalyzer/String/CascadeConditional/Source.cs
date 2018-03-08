using System;

public static class Foo
{
	public static int? Method(string value) => value?.ToString().Length;
	public static int? Method(string value, IFormatProvider formatProvider) => value?.ToString(formatProvider).Length;
}
