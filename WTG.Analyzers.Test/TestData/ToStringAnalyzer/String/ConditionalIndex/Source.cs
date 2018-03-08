using System;

public static class Foo
{
	public static char? Method(string value) => value?.ToString()[0];
	public static char? Method(string value, IFormatProvider formatProvider) => value?.ToString(formatProvider)[0];
}
