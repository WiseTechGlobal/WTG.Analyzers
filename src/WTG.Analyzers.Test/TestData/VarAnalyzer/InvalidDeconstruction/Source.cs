public class Bob
{
	void Method()
	{
		(int, short) = Frob();
		(int w ) = Frob();
		(int x, ) = Frob();
		(, short y) = Frob();
		(int a, short b, bool c) = Frob();
	}

	(int a, short b) Frob() => (1, 2);
}
