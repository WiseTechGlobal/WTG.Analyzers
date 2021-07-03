using System;

public static class Foo
{
	public static void ArrayCreator()
	{
		var o = Array.Empty<object>();
		var s = Array.Empty<string>();
		var i = Array.Empty<int>();
		var ni = Array.Empty<int?>();
	}

	public static void GenericArrayCreator<T>()
	{
		var t = Array.Empty<T>();
	}

	public static void WithFunkyWhitespace()
	{
		var t =
			Array.Empty<object>()
		;
	}
}
