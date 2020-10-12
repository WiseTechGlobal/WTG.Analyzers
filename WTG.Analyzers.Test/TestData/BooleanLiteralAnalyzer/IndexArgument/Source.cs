using System.Collections.Generic;

public static class Bob
{
	public static void Method(Dictionary<bool, object> dict)
	{
		Foo(dict[true]);
		Foo(dict[false]);
	}

	public static void Foo(object arg)
	{
	}
}
