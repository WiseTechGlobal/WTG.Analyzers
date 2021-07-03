public class Bob
{
	public Bob(bool thing1, bool thing2)
	{
	}

	public static void Method()
	{
		Bar(new Bob(true, false));

		Bar(new Bob(true,
			false));
	}

	public static void Bar(Bob b)
	{
	}
}
