using System;

public class Bob
{
	public string Method1(bool flag1, bool flag2) => flag1 ? flag2 ? "A" : "B" : "C";
	public string Method2(bool flag1, bool flag2) => flag1 ? (flag2 ? "A" : "B") : "C";

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
