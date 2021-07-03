using System;

public struct Bob
{
	public static bool Method1(Guid value) => true;
	public static bool Method2(Bob value) => true;
	public static bool Method3(Guid value) => true;
	public static bool Method4(Bob value) => true;

	public static bool operator ==(Bob a, Bob b) => false;
	public static bool operator !=(Bob a, Bob b) => false;
}
