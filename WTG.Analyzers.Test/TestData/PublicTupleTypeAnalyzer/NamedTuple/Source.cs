using System;
using System.Collections.Generic;

public class Bob
{
	public Bob(IEnumerable<(string a, string b)> arg)
	{
	}

	public void Method((int a, int b)[] foo)
	{
	}

	public (int a, int b) Method(string source) => (0, 0);
	public (int a, int b) Property => (0, 0);
	public (int a, int b) Field;
	public (int a, string b) this[(int a, string b) i] => i;
	public event EventHandler<(int a, string b)> Event1;
	public event EventHandler<(int a, string b)> Event2 { add { } remove { } }
}
