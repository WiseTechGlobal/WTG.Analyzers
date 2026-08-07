using System;

public abstract class Bob
{
	public void M()
	{
		N((_, _) => O((_, _) => 0));
	}

	protected abstract int N(Func<int, int, int> x);
	protected abstract int O(Func<int, int, int> x);
}
