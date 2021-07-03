using System.IO;

class Foo
{
	const string GrandchildSubpathWin = "child\\grandchild";
	const string GrandchildSubpathLin = "child/grandchild";

	public static void Method()
	{
		DoThingWithFile(Path.Combine("parent", GrandchildSubpathWin));
		DoThingWithFile(Path.Combine("parent", GrandchildSubpathLin));
		DoThingWithFile(Path.Combine(GrandchildSubpathWin, "great-grandchild"));
		DoThingWithFile(Path.Combine(GrandchildSubpathLin, "great-grandchild"));
	}

	static void DoThingWithFile(string file)
	{
	}
}
