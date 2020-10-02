public static class Bob
{
	public static void Method()
	{
		Bar(true, false);

		Bar(true,
			false);
	}

	public static void Bar(bool thing1, bool thing2)
	{
	}
}
