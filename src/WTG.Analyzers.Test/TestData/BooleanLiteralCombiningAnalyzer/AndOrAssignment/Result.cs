using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool Method1(bool a)
	{
		a = true;
		return a;
	}
	public static bool Method2(bool a)
	{
		return a;
	}
	public static bool Method3(bool a)
	{
		return a;
	}
	public static bool Method4(bool a)
	{
		a = false;
		return a;
	}
}
