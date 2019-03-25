using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1(Action action)
	{
		Contract.Requires<ArgumentNullException>(action != null, nameof(action));

		action();
	}

	public void Method2(Action action)
	{
		Contract.Requires<ArgumentNullException>(action != null);

		action();
	}

	public void Method3(Action action)
	{
		Contract.Requires<ArgumentException>(action != null, "Must provide action");

		action();
	}

	public void Method4(string str)
	{
		Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(str));

		str.GetType();
	}

	public void Method5(string str)
	{
		Contract.Requires<System.ArgumentException>(!string.IsNullOrEmpty(str), "Must provide str.");

		str.GetType();
	}
}
