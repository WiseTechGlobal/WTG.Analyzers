public struct Bob
{
	public static bool IsNull1(Bob builder) => builder == null;
	public static bool IsNull2(Bob builder) => null == builder;

	public static bool operator ==(Bob a, Bob b) => true;
	public static bool operator !=(Bob a, Bob b) => false;

	public static implicit operator Bob(byte[] val) => default(Bob);
}
