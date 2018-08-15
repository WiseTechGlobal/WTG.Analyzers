using System;
using System.Collections.Generic;

public class Bob
{
	public Bob(IEnumerable<Foo> arg)
	{
	}

	public void Method(Foo[] foo)
	{
	}

	public Foo Method(string source) => default(Foo);
	public Foo Property => default(Foo);
	public Foo Field;
	public Foo this[Foo i] => i;
	public event EventHandler<Foo> Event1;
	public event EventHandler<Foo> Event2 { add { } remove { } }
}

public class Foo
{
}
