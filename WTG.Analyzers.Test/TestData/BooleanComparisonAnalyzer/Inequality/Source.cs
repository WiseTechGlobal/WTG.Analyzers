public static class Bob
{
	public static bool Method1(bool value) => value != true;
	public static bool Method2(bool value) => value != false;
	public static bool Method3(bool value) => true != value;
	public static bool Method4(bool value) => false != value;
}
