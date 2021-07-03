using System;

public static class Bob
{
	public static void M1(object value)
	{
		var _ = value ?? throw new ArgumentNullException(nameof(value));
	}

	public static void M2(object value)
	{
		object _;
		_ = value ?? throw new ArgumentNullException(nameof(value));
	}
}
