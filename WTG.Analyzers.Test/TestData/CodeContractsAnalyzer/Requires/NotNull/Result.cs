using System;

public class Bob : IBob
{
	public void Method1(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException(nameof(action));
		}

		action();
	}

	protected void Method2(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("foo");
		}

		action();
	}

	void IBob.Method(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException(nameof(action));
		}

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
