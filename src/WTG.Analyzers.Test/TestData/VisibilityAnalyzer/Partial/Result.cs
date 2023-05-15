namespace ns
{
	partial class Foo
	{
		private partial int Bar();
	}

	partial class Foo
	{
		private partial int Bar() { return default; }
	}

	class Outer
	{
		partial class Bar
		{
		}
	}

	partial class Baz
	{
	}
}
