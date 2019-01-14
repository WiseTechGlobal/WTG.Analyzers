using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1(Action action)
	{
		if (action == null)
		{
			throw new UnknownType(nameof(action));
		}

		action();
	}
}
