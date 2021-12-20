using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool Not1(bool a) => a && false;
	public static bool Not2(bool a) => a || true;
}
