using System;

[Flags]
enum Foo : long
{
	None = 0,
	A = 1,
	B = 0x10,
	C = 1 << 2,
	D = (1 << 3),
	Mask = A | B | C | D,
}
