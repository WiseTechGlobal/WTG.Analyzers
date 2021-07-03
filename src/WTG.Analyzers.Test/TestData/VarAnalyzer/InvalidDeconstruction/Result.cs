public class Bob
{
	void Method()
	{
		(int, short) = Frob();
		(int w ) = Frob();
		(var x, ) = Frob();
		(, var y) = Frob();
		(var a, var b, bool c) = Frob();
	}

	(int a, short b) Frob() => (1, 2);
}
