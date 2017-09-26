using System;

[Flags]
enum Foo
{
	None = 0,
	A = 1 << 0,
	B = 1 << 1,
	C = 1 << 2,
}

[Flags]
enum Bar
{
	A = 1 << 0,
	B = 1 << 1,
	C = 1 << 2,
}
