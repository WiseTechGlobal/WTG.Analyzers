namespace ns
{
	partial class Foo
	{
		private partial int Bar();
		public partial void FooBar();
		private partial int FooBarBaz(out int value);

		private partial void Qux();
		private partial void Quux(out int value);
	}

	partial class Foo
	{
		private partial int Bar() { return default; }
		public partial void FooBar() { }
		private partial int FooBarBaz(out int value) { value = default; return default; }

		private partial void Qux() { }
		private partial void Quux(out int value) { value = default; }
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
