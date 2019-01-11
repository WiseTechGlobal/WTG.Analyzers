using System;
using System.Diagnostics.Contracts;

public static class Bob
{
	public static void Method(out object value)
	{
		Contract.EnsuresOnThrow<InvalidOperationException>(Contract.ValueAtReturn(out value) != null);
		value = new object();
		throw new System.InvalidOperationException();
	}
}
