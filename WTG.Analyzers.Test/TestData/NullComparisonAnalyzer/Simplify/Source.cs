using System;

public struct Bob
{
	public static string Method1(Guid value) => value != null ? value.ToString() : null;
	public static string Method2(Guid value1, Guid value2) => value1 != null && value2 != null ? value1.ToString() + value2.ToString() : null;
}
