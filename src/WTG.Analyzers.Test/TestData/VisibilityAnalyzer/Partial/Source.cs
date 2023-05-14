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
		private partial class Bar
		{
		}
	}

	internal partial class Baz
	{
	}
}
