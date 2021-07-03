using System.IO;

class Foo
{
	const string ChildPath = "MyCoolDir";

	public static void Method()
	{
		DoThingWithFile(Path.Combine("C:\\Blah", "My Cool Dir"));
		DoThingWithFile(Path.Combine("/usr/bin", "vim"));
		DoThingWithFile(Path.Combine("/", "opt", "homebrew"));
		DoThingWithFile(Path.Combine(@"\\?\C:\Blah", "My Cool Dir"));
		DoThingWithFile(Path.Combine(@"\\?\UNC\server\C$\Blah", "My Cool Dir"));
		DoThingWithFile(Path.Combine(@"\\server\C$\Blah", "My Cool Dir"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
