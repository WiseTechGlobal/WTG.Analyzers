#nullable enable
using System.Collections.Generic;

public class Bob
{
	void Method()
	{
		var a = Value<object>();
		object? b = Value<object>();
		var c = Value<object?>();

		var x = Value<IEnumerable<object>>();
		IEnumerable<object?> y = Value<IEnumerable<object>>();
		var z = Value<IEnumerable<object?>>();
	}

	static T Value<T>();
}
