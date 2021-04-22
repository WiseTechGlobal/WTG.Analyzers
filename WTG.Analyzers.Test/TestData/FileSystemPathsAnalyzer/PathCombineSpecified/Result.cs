using System.IO;

class Foo
{
	const string Child = "grandchild";
	const string GrandchildFolderName = "grandchild";

	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent", "child", "grandchild"));
		DoThingWithFile(Path.Combine("parent", @"child", @"grandchild"));
		DoThingWithFile(Path.Combine("parent", "child", GrandchildFolderName));
		DoThingWithFile(Path.Combine("parent", "child", GrandchildFolderName));
		DoThingWithFile(Path.Combine("parent", @"child", GrandchildFolderName));
		DoThingWithFile(Path.Combine("parent", Child, GrandchildFolderName));
		DoThingWithFile(Path.Combine("parent", Child, GrandchildFolderName));
	}

	static void DoThingWithFile(string file)
	{
	}
}
