public static class Bob
{
	public static void Method()
	{
		Bar(thing1: true, thing2: false);

		Bar(thing1: true,
			thing2: false);
	}

	public static void Bar<T>(T thing1, T thing2)
	{
	}
}
