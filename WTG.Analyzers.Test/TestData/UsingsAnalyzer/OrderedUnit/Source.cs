using System;
using static System.Console;
using str = System.String;

public class ClassyFoo
{
	public void Foo()
	{
		str x = "1";
		if (int.TryParse(x, out var value))
		{
			WriteLine(value);
		}
	}
}
