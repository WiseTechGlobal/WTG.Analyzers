using System.IO;

class Foo
{
	const string WindowsPath = "My Path";
	const string LinuxPath = "mypath";

	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent", WindowsPath));
		DoThingWithFile(Path.Combine("parent", LinuxPath));
		DoThingWithFile(Path.Combine(WindowsPath, "child"));
		DoThingWithFile(Path.Combine(LinuxPath, "child"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
