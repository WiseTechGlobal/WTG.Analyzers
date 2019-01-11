using System;
using System.Diagnostics.Contracts;

public static class Bob
{
	public static void Method(out object value)
	{
		value = new object();
		throw new System.InvalidOperationException();
	}
}
