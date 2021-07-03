static class Bob
{
	public static void Foo(ref int a)
	{
		(a, var b) = Bar();
	}

	static (int, int) Bar() => (3, 4);
}
