using System;

public abstract class Bob
{
	public void M()
	{
		N((_, _, _) => 0);
	}

	protected abstract void N(Func<int, int, int, int> x);
}
