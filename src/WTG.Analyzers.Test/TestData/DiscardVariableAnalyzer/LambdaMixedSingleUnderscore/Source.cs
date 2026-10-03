using System;

public abstract class Bob
{
	public void M()
	{
		N((_, x) => { });
	}

	protected abstract void N(Action<int, int> x);
}
