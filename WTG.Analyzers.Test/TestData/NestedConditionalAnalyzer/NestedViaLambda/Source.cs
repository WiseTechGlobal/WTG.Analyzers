using System;

public class Bob
{
	public Func<string> Method3(bool flag1, bool flag2) => flag1
		? () => flag2 ? "A" : "B"
		: (Func<string>)(() => "C");

	public Func<string> Method4(bool flag1, bool flag2) => flag1
		? () =>
		{
			return flag2 ? "A" : "B";
		}
		: (Func<string>)(() => "C");
}
