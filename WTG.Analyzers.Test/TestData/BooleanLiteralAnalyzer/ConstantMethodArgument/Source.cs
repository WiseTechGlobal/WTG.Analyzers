public static class Bob
{
	const bool TrueConst = true;
	const bool FalseConst = false;

	public static void Method()
	{
		Bar(TrueConst, FalseConst);

		Bar(TrueConst,
			FalseConst);
	}

	public static void Bar(bool thing1, bool thing2)
	{
	}
}
