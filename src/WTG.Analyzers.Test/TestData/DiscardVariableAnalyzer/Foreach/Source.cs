using System.Collections.Generic;

public abstract class Bob
{
	public void M()
	{
		foreach (var _ in N())
		{
		}
	}

	protected abstract IEnumerable<int> N();
}
