using System;

[Flags]
enum Foo
{
	None = 0,
	A = 1,
	X1,
	B = 0x02,
	X2,
	C = 1 << 2,
	X3,
	D = (1 << 3),
}
