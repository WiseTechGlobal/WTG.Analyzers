class Bob
{
	public static void Foo()
	{
		(var a, var b) = Bar();
	}

	static Bob Bar() => new Bob();

	public void Deconstruct(out int x, out int y)
	{
		x = 3;
		y = 4;
	}
}
