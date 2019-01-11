using System.Diagnostics.Contracts;

public class Bob : IBob
{
	public void Method1(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new System.ArgumentException("Value cannot be null or empty.", nameof(value));
		}

		value.GetType();
	}

	protected void Method2(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new System.ArgumentException("Invalid value.", nameof(value));
		}

		value.GetType();
	}

	void IBob.Method(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new System.ArgumentException("Value cannot be null or empty.", nameof(value));
		}

		value.GetType();
	}

	void Method3(string value)
	{
		value.GetType();
	}
}

interface IBob
{
	void Method(string value);
}
