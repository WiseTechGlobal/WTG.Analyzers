public static class Foo
{
	public static void ArrayCreator()
	{
		var o = new object[][] { };
		var s = new string[][] { };
		var i = new int[][] { };
		var ni = new int?[][] { };
	}

	public static void GenericArrayCreator<T>()
	{
		var t = new T[][] { };
	}
}
