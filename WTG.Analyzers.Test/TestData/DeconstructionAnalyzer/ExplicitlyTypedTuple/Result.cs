static class Bob
{
	public static void Foo()
	{
		(int a, int b) = Bar();
	}

	static (int, int) Bar() => (3, 4);
}
