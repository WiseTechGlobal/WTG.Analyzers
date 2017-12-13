public static class Foo
{
	public static void ArrayCreator()
	{
		var o = new object[0, 0];
		var s = new string[0,0];
		var i = new int[1,2];
		var ni = new int?[3, 2];
	}

	public static void GenericArrayCreator<T>()
	{
		var t = new T[0, 0];
	}
}
