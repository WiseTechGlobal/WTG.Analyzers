using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public Action Action { get; }

	public void Method1()
	{
		Contract.Requires(Action != null);

		Action();
	}
}
