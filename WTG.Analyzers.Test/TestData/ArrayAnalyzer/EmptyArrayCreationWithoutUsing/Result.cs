public static class Foo
{
	public static void ArrayCreator()
	{
		var o = System.Array.Empty<object>();
		var s = System.Array.Empty<string>();
		var i = System.Array.Empty<int>();
		var ni = System.Array.Empty<int?>();
	}

	public static void GenericArrayCreator<T>()
	{
		var t = System.Array.Empty<T>();
	}
}
