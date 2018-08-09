using System;
using System.Collections.Generic;

public class Bob
{
	public Bob(IEnumerable<Tuple<string, string>> arg)
	{
	}

	public void Method(Tuple<int, int>[] foo)
	{
	}

	public Tuple<int, int> Method(string source) => default(Tuple<int, int>);
	public Tuple<int, int> Property => default(Tuple<int, int>);
	public Tuple<int, int> Field;
	public Tuple<int, string> this[Tuple<int, string> i] => i;
	public event EventHandler<Tuple<int, string>> Event1;
	public event EventHandler<Tuple<int, string>> Event2 { add { } remove { } }
}
