static class Bob
{
	public static void Foo()
	{
		var (a, b) = Bar();
	}

	static (int, int) Bar() => (3, 4);
}
