using System;

public abstract class Bob
{
	public void M()
	{
		N(_ => { });
		N((_) => { });
	}

	protected abstract void N(Action<int> x);
}
