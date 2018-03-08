using System;

public static class Foo
{
	public static char? Method(string value) => value?[0];
	public static char? Method(string value, IFormatProvider formatProvider) => value?[0];
}
