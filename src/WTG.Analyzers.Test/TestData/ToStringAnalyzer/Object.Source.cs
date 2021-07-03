using System;

public static class Foo
{
	// Don't suggest removing ToString() for just any type.
	public static string Method(object value) => value.ToString();
}
