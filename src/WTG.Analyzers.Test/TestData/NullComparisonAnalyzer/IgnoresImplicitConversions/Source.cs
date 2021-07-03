public struct Bob
{
	public static bool IsNull1(Bob builder) => builder == null;
	public static bool IsNull2(Bob builder) => null == builder;

	public static bool operator ==(Bob a, Bob b) => true;
	public static bool operator !=(Bob a, Bob b) => false;

	public static implicit operator Bob(byte[] val) => default(Bob);
}

public struct Wendy
{
	public static bool IsNull1(Wendy w) => w == null;
	public static bool IsNull2(Wendy w) => null == w;

	public static bool operator ==(Wendy a, Wendy b) => true;
	public static bool operator !=(Wendy a, Wendy b) => false;

	public static implicit operator Wendy(int? i) => default(Wendy);
}
