using System.Diagnostics.Contracts;

public class Bob : IBob
{
	public void Method1(int value)
	{
		if (value <= 3)
		{
			throw new System.ArgumentException("Invalid Argument.", nameof(value));
		}

		value.GetType();
	}

	protected void Method2(int value)
	{
		if (value <= 3)
		{
			throw new System.ArgumentException("Message", nameof(value));
		}

		value.GetType();
	}

	void IBob.Method(int value)
	{
		if (value <= 3)
		{
			throw new System.ArgumentException("Invalid Argument.", nameof(value));
		}

		value.GetType();
	}

	void Method3(int value)
	{
		value.GetType();
	}
}

interface IBob
{
	void Method(int value);
}
