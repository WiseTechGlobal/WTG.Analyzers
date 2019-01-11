using System.Diagnostics.Contracts;

public static class Bob
{
	[ContractVerification(false)]
	public static bool Method(int value) => value == 42;
}
