using System;
using System.Collections.Generic;

public interface IBob
{
	void Method((int a, int b)[] foo);
	(int a, int b) Method(string source);
	(int a, int b) Property { get; }
	(int a, string b) this[(int a, string b) i] { get; }
	event EventHandler<(int a, string b)> Event1;
	event EventHandler<(int a, string b)> Event2;
}

public class Bob : IBob
{
	public void Method((int a, int b)[] foo)
	{
	}

	public (int a, int b) Method(string source) => (0, 0);
	public (int a, int b) Property => (0, 0);
	public (int a, string b) this[(int a, string b) i] => i;
	public event EventHandler<(int a, string b)> Event1;
	public event EventHandler<(int a, string b)> Event2 { add { } remove { } }
}
