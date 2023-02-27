using System.Linq;

public class Foo
{
	string Method1() => string.Concat("Hello, World!", "!");

	WorstObject Method2() => new WorstObject().Concat(new[] { new WorstObject() });
}

public class WorstObject
{
	public WorstObject Concat(WorstObject[] con) => throw null;
}
