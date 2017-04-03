public static class Bob
{
	public static bool Method(bool value)
	{
		Magic(value);
		Magic(!value);
	}

	static void Magic(bool argument)
	{
	}
}
