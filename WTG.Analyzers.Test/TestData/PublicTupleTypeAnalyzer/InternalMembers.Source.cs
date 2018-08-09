using System;
using System.Collections.Generic;

public class Bob
{
	internal Bob(IEnumerable<(string a, string b)> arg)
	{
	}

	internal void Method((int a, int b)[] foo)
	{
	}

	internal (int a, int b) Method(string source) => (0, 0);
	internal (int a, int b) Property => (0, 0);
	internal (int a, int b) Field;
	internal (int a, string b) this[(int a, string b) i] => i;
	internal event EventHandler<(int a, string b)> Event1;
	internal event EventHandler<(int a, string b)> Event2 { add { } remove { } }
}
