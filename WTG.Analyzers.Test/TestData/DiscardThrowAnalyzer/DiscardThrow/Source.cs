using System;

public static class Bob
{
	public static void M(object value)
	{
		_ = value ?? throw new ArgumentNullException(nameof(value));
	}

	public static void M(object value1, object value2)
	{
		_ = value1 ?? throw new ArgumentNullException(nameof(value1));
		_ = value2 ?? throw new ArgumentNullException(nameof(value2));
	}
}
