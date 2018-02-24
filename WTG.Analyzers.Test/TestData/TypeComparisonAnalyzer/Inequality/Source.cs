using System;

public static class Bob
{
	public static bool Method1(object value)
	{
		return value.GetType() != typeof(Guid?);
	}

	public static bool Method2(object value)
	{
		return typeof(Guid?) != value.GetType();
	}

	static Guid probe; // keep the using relevant after removing the other Guid references.
}
