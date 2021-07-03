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

namespace System.IO
{
	public static class Path
	{
		public static string Combine(string path1, string path2) { throw null; }
	}
}
