using System.Diagnostics.Contracts;

public static class Bob
{
	public static object Method()
	{
		Contract.Ensures(Contract.Result<object>() != null);
		return new object();
	}
}
