public static class Bob
{
	public static void Method(bool value)
	{
		Magic(value == true);
		Magic(value == false);
	}

	static void Magic(bool argument)
	{
	}
}
