using System;

public static class Bob
{
	public static bool Method1(object value)
	{
		return false;
	}

	public static bool Method2(object value)
	{
		return false;
	}

	static Guid probe; // keep the using relevant after removing the other Guid references.
}
