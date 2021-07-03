using System.IO;

class Foo
{
	const string Child = "grandchild";
	const string GrandchildFolderName = "grandchild";

	public static void Method()
	{
		DoThingWithFile(Path.Combine(new[] { "parent", "child", "grandchild" }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", "grandchild" }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", GrandchildFolderName }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", GrandchildFolderName }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", GrandchildFolderName }));
		DoThingWithFile(Path.Combine(new[] { "parent", Child, GrandchildFolderName }));
		DoThingWithFile(Path.Combine(new[] { "parent", Child, GrandchildFolderName }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", "grandchild" + 123 }));
		DoThingWithFile(Path.Combine(new[] { "parent", 123 + "child", "grandchild" }));
		DoThingWithFile(Path.Combine(new[] { "parent", "child", $"prefixed{GrandchildFolderName}.001" }));
	}

	static void DoThingWithFile(string file)
	{
	}
}
