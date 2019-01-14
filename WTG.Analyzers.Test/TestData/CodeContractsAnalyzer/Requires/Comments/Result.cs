using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1(Action action1, Action action2, string message)
	{
		// Comment1
		if (action1 == null)
		{
			throw new ArgumentNullException(nameof(action1));
		}

		// Comment2
		if (action2 == null)
		{
			throw new ArgumentNullException(nameof(action2));
		}

		// Comment3
		if (string.IsNullOrEmpty(message))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(message));
		}

		// Comment4
		action1();
		action2();
	}
}
