public abstract class Bob
{
	public void M()
	{
		N(out var _);
	}

	protected abstract void N(out int value);
}
