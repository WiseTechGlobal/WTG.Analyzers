using System;
using System.Collections.Generic;

public class Bob
{
	Bob(IEnumerable<(string a, string b)> arg)
	{
	}

	void Method((int a, int b)[] foo)
	{
	}

	(int a, int b) Method(string source) => (0, 0);
	(int a, int b) Property => (0, 0);
	(int a, int b) Field;
	(int a, string b) this[(int a, string b) i] => i;
	event EventHandler<(int a, string b)> Event1;
	event EventHandler<(int a, string b)> Event2 { add { } remove { } }
}
