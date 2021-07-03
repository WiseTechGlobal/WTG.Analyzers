using System;
using System.Collections.Generic;

public class Bob
{
	public Bob(IEnumerable<ValueTuple<string, string>> arg)
	{
	}

	public void Method(ValueTuple<int, int>[] foo)
	{
	}

	public ValueTuple<int, int> Method(string source) => default(ValueTuple<int, int>);
	public ValueTuple<int, int> Property => default(ValueTuple<int, int>);
	public ValueTuple<int, int> Field;
	public ValueTuple<int, string> this[ValueTuple<int, string> i] => i;
	public event EventHandler<ValueTuple<int, string>> Event1;
	public event EventHandler<ValueTuple<int, string>> Event2 { add { } remove { } }
}

namespace System
{
	public struct ValueTuple<T1, T2>
	{
	}
}
