using System;

public static class Bob
{
	public static void M(object value1, object value2)
	{
		// leading 1
		_ = value1 ?? throw new ArgumentNullException(nameof(value1)); // eol 1

		//leading 2
		_ = value2 ?? throw new ArgumentNullException(nameof(value2)); // eol 2
	}
}
