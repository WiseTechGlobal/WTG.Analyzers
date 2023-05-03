partial class Foo
{
	private partial int Bar();
}

partial class Foo
{
	private partial int Bar() { return default; }
}
