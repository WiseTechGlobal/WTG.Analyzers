partial class Foo
{
	private partial int Bar();
	partial private int Baz();
}

partial class Foo
{
	private partial int Bar() { return default; }
	partial private int Baz() { return default; }
}
