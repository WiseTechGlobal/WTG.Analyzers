using System;

public class Bob
{
	public void DisposeSameType(Func<Context> bob)
	{
		using (var n = bob())
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

public class Context : IDisposable
{
	public void Dispose()
	{
	}
}
