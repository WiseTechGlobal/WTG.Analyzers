using System.IO;

class Foo
{
	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent", "child", "grandchild"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
