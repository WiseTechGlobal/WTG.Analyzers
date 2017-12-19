public static class Foo
{
	public static void StaticArrayCreator<T>()
	{
		Method(new int[0]);
		Method(new int[] { });
	}

	public static void Method(int[] argument)
	{
	}
}
