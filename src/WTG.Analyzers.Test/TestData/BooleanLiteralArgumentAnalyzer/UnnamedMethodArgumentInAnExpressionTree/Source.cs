using System;
using System.Linq.Expressions;

public class Bob
{
	public static void Method()
	{
		Baz(b => b.Bar(true, false));

		Baz(b => b.Bar(true,
			false));
	}

	public void Bar(bool thing1, bool thing2)
	{
	}

	public static void Baz(Expression<Action<Bob>> expr) { }
}
