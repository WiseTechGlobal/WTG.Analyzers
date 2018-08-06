public struct Bob
{
	public static bool IsNull1(Bob builder) => builder == null;
	public static bool IsNull2(Bob builder) => null == builder;

	public static bool operator ==(Bob a, object b) => true;
	public static bool operator !=(Bob a, object b) => false;

	public static bool operator ==(object a, Bob b) => true;
	public static bool operator !=(object a, Bob b) => false;
}
