using System.IO;

class Foo
{
	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent" + Path.DirectorySeparatorChar + "child", "grandchild"));
		DoThingWithFile(Path.Combine("parent" + Path.AltDirectorySeparatorChar + "child", "grandchild"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
