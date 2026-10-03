using System;

public abstract class Bob
{
	public void M()
	{
		N(delegate(int _, int _) { });
	}

	protected abstract void N(Action<int, int> x);
}
