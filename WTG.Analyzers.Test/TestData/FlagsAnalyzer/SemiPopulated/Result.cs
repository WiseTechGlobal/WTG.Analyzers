using System;

[Flags]
enum Foo
{
	None = 0,
	A = 1,
	X1 = 1 << 4,
	B = 0x02,
	X2 = 1 << 5,
	C = 1 << 2,
	X3 = 1 << 6,
	D = (1 << 3),
}
