public static class Bob
{
#if true
	public static bool Method(bool a) => a || false;
#endif
}
