using System;

static class Bob
{
	public static void Foo1()
	{
		var ((v, w), x, (y, z)) = Bar();
	}

	public static void Foo2()
	{
		var ((v, w), x, (y, z)) = Bar();
	}

	static ((int a, int b), int c, (int d, int e) f) Bar() => throw new NotImplementedException();
}
