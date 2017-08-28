static class Bob
{
	public static void Foo(ref int a, ref int b)
	{
		(a, b) = Bar();
	}

	static (int, int) Bar() => (3, 4);
}
