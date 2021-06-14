using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool And1(bool a) => a && true;
	public static bool And2(bool a) => true && a;
	public static bool Or1(bool a) => a || false;
	public static bool Or2(bool a) => false || a;
}
