using System;

public struct Bob
{
	public static bool Method(Guid value) =>
		value == null; // This is a useful comment.

	public static bool operator ==(Bob a, Bob b) => false;
	public static bool operator !=(Bob a, Bob b) => false;
}
