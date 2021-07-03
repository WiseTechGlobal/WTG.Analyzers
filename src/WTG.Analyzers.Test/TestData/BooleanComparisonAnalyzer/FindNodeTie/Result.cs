public static class Bob
{
	public static void Method(bool value)
	{
		Magic(value);
		Magic(!value);
	}

	static void Magic(bool argument)
	{
	}
}
