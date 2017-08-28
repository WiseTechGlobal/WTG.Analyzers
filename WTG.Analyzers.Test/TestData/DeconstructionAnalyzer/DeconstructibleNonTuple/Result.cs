class Bob
{
	public static void Foo()
	{
		var (a, b) = Bar();
	}

	static Bob Bar() => new Bob();

	public void Deconstruct(out int x, out int y)
	{
		x = 3;
		y = 4;
	}
}
