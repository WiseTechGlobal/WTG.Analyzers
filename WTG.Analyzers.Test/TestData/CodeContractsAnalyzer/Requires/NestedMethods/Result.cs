using System;

public class Bob
{
	public void LocalMethod(Action action)
	{
		DoStuff(action);

		void DoStuff(Action x)
		{
			x();
		}
	}

	public void AnonymousMethod(Action action)
	{
		X(delegate (Action x)
		{
			x();
		});
	}

	public void SimpleLambda(Action action)
	{
		X(x =>
		{
			x();
		});
	}

	public void PLambda(Action action)
	{
		X((x) =>
		{
			x();
		});
	}

	void X(Action<Action> action) => action(null);
}
