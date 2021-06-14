public static class Bob
{
	public static void Method()
	{
		Bar(true, thing2: false);

		Bar(true,
			thing2: false);
	}

	public static void Bar(bool thing1, bool thing2)
	{
	}
}
