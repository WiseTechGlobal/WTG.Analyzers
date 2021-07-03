using System;

public static class Foo
{
	public static void ArrayCreator()
	{
		var o = new object[0];
		var s = new string[0];
		var i = new int[0];
		var ni = new int?[0];
	}

	public static void GenericArrayCreator<T>()
	{
		var t = new T[0];
	}

	public static void WithFunkyWhitespace()
	{
		var t =
			new
				object[
					0
						]
		;
	}
}
