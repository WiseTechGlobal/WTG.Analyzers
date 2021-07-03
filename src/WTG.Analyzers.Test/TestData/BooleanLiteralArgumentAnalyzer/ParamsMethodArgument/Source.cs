public static class Bob
{
	public static void Method()
	{
		Foo(null, true, false);
		Bar(true, false);
		Baz(true, false);
	}

	public static void Foo(params object[] args)
	{
	}

	public static void Bar(params bool[] args)
	{
	}

	public static void Baz<T>(params T[] args)
	{
	}
}
