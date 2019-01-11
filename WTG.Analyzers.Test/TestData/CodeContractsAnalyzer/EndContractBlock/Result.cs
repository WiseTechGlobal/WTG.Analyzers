using System;
using System.Diagnostics.Contracts;

public static class Bob
{
	public static void Method(Action arg)
	{
		if (arg == null)
		{
			throw new ArgumentNullException();
		}

		arg();
	}
}
