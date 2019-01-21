using System.Diagnostics.Contracts;

public class Bob : IBob
{
	public void Method1(string value)
	{
		Contract.Requires(!string.IsNullOrEmpty(value));

		value.GetType();
	}

	protected void Method2(string value)
	{
		Contract.Requires(!string.IsNullOrEmpty(value), "Invalid value.");

		value.GetType();
	}

	void IBob.Method(string value)
	{
		Contract.Requires(!string.IsNullOrEmpty(value));

		value.GetType();
	}

	void Method3(string value)
	{
		Contract.Requires(!string.IsNullOrEmpty(value));

		value.GetType();
	}
}

interface IBob
{
	void Method(string value);
}
