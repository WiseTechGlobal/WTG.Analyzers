using System.Diagnostics.Contracts;

public class Bob
{
	public void Method1()
	{
		Contract.Requires();
	}

	public void Method2()
	{
		Contract.Requires(,);
	}

	public void Method3(int value)
	{
		if (value <= 3)
		{
			throw new System.ArgumentException("Invalid Argument.", nameof(value));
		}
	}

	public void Method4(int value)
	{
		Contract.Requires <,> (value > 3);
	}
}
