using System.IO;

class Foo
{
	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent", "foo\\bar"));
		DoThingWithFile(Path.Combine("parent", "foo/bar"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
