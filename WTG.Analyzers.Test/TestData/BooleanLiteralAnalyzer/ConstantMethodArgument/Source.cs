public static class Bob
{
	const bool TrueConst = true;
	const bool FalseConst = false;

	public static void Method()
	{
		Bar(thing1: TrueConst, thing2: FalseConst);

		Bar(thing1: TrueConst,
			thing2: FalseConst);
	}

	public static void Bar(bool thing1, bool thing2)
	{
	}
}
