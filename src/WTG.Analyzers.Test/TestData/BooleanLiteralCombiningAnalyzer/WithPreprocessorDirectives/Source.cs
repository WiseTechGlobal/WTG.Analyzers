public static class Bob
{
	public static bool Conditional(bool? something)
	{
		return
#if true
			something.HasValue ? something.Value :
#endif
			true;
	}

	public static bool Or(bool value0, bool value1, bool value2)
	{
		return false
#if true
			|| (
				value0 &&
				value1 &&
				value2
			)
#endif
			;
	}

	public static bool And(bool value0, bool value1) => true
#if true
		&& !(value0 && value1)
#endif
		;

	public static bool Region(bool value)
	{
		return
#region Keep this directive
			value || false
#endregion
			;
	}
}
