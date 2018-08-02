public static class Bob
{
	public static bool Method1(long? value) => value == null;
	public static bool Method2(System.Guid? value) => value != null;
}
