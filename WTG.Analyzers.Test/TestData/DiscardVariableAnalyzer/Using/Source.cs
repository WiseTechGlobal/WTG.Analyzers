using System;

public abstract class Bob
{
	public void M()
	{
		using (var _ = N())
		{
		}
	}

	protected abstract IDisposable N();
}
