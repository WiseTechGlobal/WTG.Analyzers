using System;

public abstract class Bob
{
	public void M()
	{
		N((int _, int _) => 0);
	}

	protected abstract void N(Func<int, int, int> x);
}
