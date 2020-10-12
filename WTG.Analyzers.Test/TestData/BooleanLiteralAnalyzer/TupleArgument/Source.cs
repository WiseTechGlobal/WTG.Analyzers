public static class Bob
{
	public static void Method()
	{
		Foo((true, false));
	}

	public static void Foo((bool, bool) args)
    {
    }
}
