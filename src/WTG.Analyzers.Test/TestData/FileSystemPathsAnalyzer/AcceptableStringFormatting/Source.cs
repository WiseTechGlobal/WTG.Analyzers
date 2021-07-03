using System.IO;

class Foo
{
	public static void Method(int a)
	{
		DoThingWithFile(Path.Combine("parent", $"child{a}"));
		DoThingWithFile(Path.Combine("parent", @"abcd"));
		DoThingWithFile(Path.Combine("parent", $@"abc{a}"));
		DoThingWithFile(Path.Combine("parent", "child" + a.ToString("X")));
	}

	static void DoThingWithFile(string file)
	{
	}
}
