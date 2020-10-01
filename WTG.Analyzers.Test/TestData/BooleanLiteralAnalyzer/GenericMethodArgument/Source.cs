public static class Bob
{
	public static void Method()
	{
		Bar(true, false);

		Bar(true,
			false);
	}

	public static void Bar<T>(T thing1, T thing2)
	{
	}
}
