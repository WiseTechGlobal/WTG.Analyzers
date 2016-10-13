using System;

public class Bob
{
	public void DisposeSameType(Func<Context> bob)
	{
		using (Context n = bob())
		{
		}
	}

	public void DisposeDifferentType(Func<Context> bob)
	{
		// I'm not sure there is much point letting people specify a more generic type in a using since it can't be re-assigned
		// ... but lets leave it for now.
		using (IDisposable n = bob())
		{
		}
	}

	public void UsingWithoutLocal()
	{
		using (new Context())
		{
		}
	}
}

class Context : IDisposable
{
	void Dispose()
	{
	}
}
