using System;

public class Bob
{
	public void Method1(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException(nameof(action));
		}

		action();
	}

	public void Method2(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException(nameof(action));
		}

		action();
	}

	public void Method3(Action action)
	{
		if (action == null)
		{
			throw new ArgumentException("Must provide action");
		}

		action();
	}

	public void Method4(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(str));
		}

		str.GetType();
	}

	public void Method5(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			throw new ArgumentException("Must provide str.");
		}

		str.GetType();
	}
}
