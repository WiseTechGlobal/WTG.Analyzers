using System;
using System.Diagnostics.Contracts;

public class Bob : IBob
{
	public void Method1(Action action)
	{
		Contract.Requires(action != null);

		action();
	}

	protected void Method2(Action action)
	{
		Contract.Requires(action != null);

		action();
	}

	void IBob.Method(Action action)
	{
		Contract.Requires(action != null);

		action();
	}

	void Method3(Action action)
	{
		action();
	}
}

interface IBob
{
	void Method(Action action);
}
