namespace ns
{
	partial class Foo
	{
		private partial int Bar();
		public partial void FooBar();

		partial void Qux();
		private partial void Quux(out int value);
	}

	partial class Foo
	{
		private partial int Bar() { return default; }
		public partial void FooBar() { }

		partial void Qux() { }
		private partial void Quux(out int value) { value = default; }
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
