using System;
using System.Collections.Generic;

public abstract class BaseBob
{
	public abstract void Method((int a, int b)[] foo);
	public abstract (int a, int b) Method(string source);
	public abstract (int a, int b) Property { get; }
	public abstract (int a, string b) this[(int a, string b) i] { get; }
	public abstract event EventHandler<(int a, string b)> Event1;
	public abstract event EventHandler<(int a, string b)> Event2;
}

public class Bob : BaseBob
{
	public override void Method((int a, int b)[] foo)
	{
	}

	public override (int a, int b) Method(string source) => (0, 0);
	public override (int a, int b) Property => (0, 0);
	public override (int a, string b) this[(int a, string b) i] => i;
	public override event EventHandler<(int a, string b)> Event1;
	public override event EventHandler<(int a, string b)> Event2 { add { } remove { } }
}
