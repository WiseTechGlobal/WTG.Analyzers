using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool Conditional1(bool a, bool b) => a;
	public static bool Conditional2(bool a, bool b) => b;
	public static bool Conditional3(bool a, bool b) => a || b;
	public static bool Conditional4(bool a, bool b) => !a && b;
	public static bool Conditional5(bool a, bool b) => !a || b;
	public static bool Conditional6(bool a, bool b) => a && b;
}
