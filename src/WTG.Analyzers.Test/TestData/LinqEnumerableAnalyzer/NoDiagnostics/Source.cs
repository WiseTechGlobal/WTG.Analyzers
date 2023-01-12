using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

public class Foo
{
	// string.Concat() is not an extension receiver method - so we don't have to worry because we're already checking Enumerable type
	string Method1() => string.Concat("Hello, World!", "!");

	// this, however, does raise a diagnostic instance and that's kind of a problem...
	WorstObject Method2()
	{
		return new WorstObject("why?", new[] { "continuing to ask why?" }).Concat(new[] { new WorstObject("why?", new[] { "continuing to ask why?" }) });
	}
}

public class WorstObject
{
	public string name;
	public string[] properties;

	public WorstObject(string name, string[] properties)
	{
		this.name = name;
		this.properties = properties;
	}

	public WorstObject Concat(WorstObject[] con)
	{
		var name = this.name;
		var properties = this.properties;

		foreach (var why in con)
		{
			name += why.name;
			properties.Concat(why.properties);
		}

		return new WorstObject(name, properties);
	}
}
