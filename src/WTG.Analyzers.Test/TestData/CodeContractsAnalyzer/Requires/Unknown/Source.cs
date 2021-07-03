using System.Diagnostics.Contracts;

public class Bob : IBob
{
	public void Method1(int value)
	{
		Contract.Requires(value > 3);

		value.GetType();
	}

	protected void Method2(int value)
	{
		Contract.Requires(value > 3, "Message");

		value.GetType();
	}

	void IBob.Method(int value)
	{
		Contract.Requires(value > 3);

		value.GetType();
	}

	void Method3(int value)
	{
		Contract.Requires(value > 3);

		value.GetType();
	}
}

interface IBob
{
	void Method(int value);
}
