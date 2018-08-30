using System;

public struct Bob
{
	public static string Method1(Guid value) => value.ToString();
	public static string Method2(Guid value1, Guid value2) => value1.ToString() + value2.ToString();
}
