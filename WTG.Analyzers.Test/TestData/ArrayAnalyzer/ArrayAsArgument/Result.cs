public static class Foo
{
	public static void StaticArrayCreator<T>()
	{
		Method(System.Array.Empty<int>());
		Method(System.Array.Empty<int>());
	}

	public static void Method(int[] argument)
	{
	}
}
