using System.Collections.Generic;

public abstract class Bob
{
	public void M1()
	{
		var (x, _) = N1();
	}

	public void M2()
	{
		(var x, _) = N1();
	}

	public void M3()
	{
		(var x, var _) = N1(); // '_' is still treated as a discard here, even though 'var' makes it look like a local.
	}

	public void M4()
	{
		foreach (var (x, _) in N2())
		{
		}
	}

	protected abstract (int a, string b) N1();
	protected abstract IEnumerable<(int a, string b)> N2();
}
