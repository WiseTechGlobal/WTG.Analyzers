using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1(Action action1, Action action2, string message)
	{
		// Comment1
		Contract.Requires(action1 != null);

		// Comment2
		Contract.Requires<ArgumentNullException>(action2 != null, nameof(action2));

		// Comment3
		Contract.Requires(!string.IsNullOrEmpty(message));

		// Comment4
		action1();
		action2();
	}
}
