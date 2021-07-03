using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public void LocalMethod(Action action)
	{
		DoStuff(action);

		void DoStuff(Action x)
		{
			Contract.Requires(x != null);
			Contract.Requires(action != null);
			x();
		}
	}

	public void AnonymousMethod(Action action)
	{
		X(delegate (Action x)
		{
			Contract.Requires(x != null);
			Contract.Requires(action != null);
			x();
		});
	}

	public void SimpleLambda(Action action)
	{
		X(x =>
		{
			Contract.Requires(x != null);
			Contract.Requires(action != null);
			x();
		});
	}

	public void PLambda(Action action)
	{
		X((x) =>
		{
			Contract.Requires(x != null);
			Contract.Requires(action != null);
			x();
		});
	}

	void X(Action<Action> action) => action(null);
}
