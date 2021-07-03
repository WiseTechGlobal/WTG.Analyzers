using System;

static class Bob
{
	public static void Foo1()
	{
		((var v, var w), var x, (var y, var z)) = Bar();
	}

	public static void Foo2()
	{
		(var (v, w), var x, var (y, z)) = Bar();
	}

	static ((int a, int b), int c, (int d, int e) f) Bar() => throw new NotImplementedException();
}
