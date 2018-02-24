using System;

public static class Bob
{
	public static bool Method1(object value)
	{
		return true;
	}

	public static bool Method2(object value)
	{
		return true;
	}

	static Guid probe; // keep the using relevant after removing the other Guid references.
}
