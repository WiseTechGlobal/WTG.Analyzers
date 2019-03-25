using System.Diagnostics.Contracts;

public static class Bob
{
	public static object Method()
	{
		var value = new object();
		Contract.Assume(value != null);
		return value;
	}
}
