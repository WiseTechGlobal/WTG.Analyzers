using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool And(bool a, bool b) => a && b;
	public static bool Or(bool a, bool b) => a || b;
	public static bool Not(bool a) => !a;
	public static bool True() => true;
	public static bool False() => false;

	public static bool Assign(bool a, bool b)
	{
		a |= b;
		b &= a;
		return b;
	}
}
