using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1(Action action)
	{
		Contract.Requires<UnknownType>(action != null, nameof(action));

		action();
	}
}
