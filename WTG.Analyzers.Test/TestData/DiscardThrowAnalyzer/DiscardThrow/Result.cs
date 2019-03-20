using System;

public static class Bob
{
	public static void M(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}
	}

	public static void M(object value1, object value2)
	{
		if (value1 == null)
		{
			throw new ArgumentNullException(nameof(value1));
		}
		if (value2 == null)
		{
			throw new ArgumentNullException(nameof(value2));
		}
	}
}
