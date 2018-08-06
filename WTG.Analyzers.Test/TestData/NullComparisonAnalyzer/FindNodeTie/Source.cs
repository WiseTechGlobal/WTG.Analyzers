public static class Bob
{
	public static void Method(System.Guid value)
	{
		Magic(value == null);
		Magic(value != null);
	}

	static void Magic(bool argument)
	{
	}
}
