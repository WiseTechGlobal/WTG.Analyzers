static class Bob
{
	public static void Foo()
	{
		(var a, var b) = Bar();
	}

	static (int, int) Bar() => (3, 4);
}
