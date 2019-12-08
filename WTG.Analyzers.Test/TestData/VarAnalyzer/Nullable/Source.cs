#nullable enable
using System.Collections.Generic;

public class Bob
{
	void Method()
	{
		object a = Value<object>();
		object? b = Value<object>();
		object? c = Value<object?>();

		IEnumerable<object> x = Value<IEnumerable<object>>();
		IEnumerable<object?> y = Value<IEnumerable<object>>();
		IEnumerable<object?> z = Value<IEnumerable<object?>>();
	}

	static T Value<T>() => default!;
}
