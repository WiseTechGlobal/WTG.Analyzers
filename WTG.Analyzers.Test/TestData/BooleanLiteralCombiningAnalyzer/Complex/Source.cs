using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool Complex(bool a, bool b)
		=> a || false ? (!true && a) : b && true;
}
