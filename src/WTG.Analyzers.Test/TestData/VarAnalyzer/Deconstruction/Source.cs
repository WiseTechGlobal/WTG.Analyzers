public class Bob
{
	void WellKnown()
	{
		(int x1, short x2) = Frob();
		(int y1, int y2) = (2, 3);
		((int z1, short z2), (double z3, char z4)) = DeeplyDisturbed();
	}

	void NotWellKnown()
	{
		(int x1, int x2) = Frob();
		(int y1, short y2) = (2, 3);
	}

	void AlreadyVar()
	{
		(var x1, var x2) = Frob();
		var (y1, y2) = Frob();
		var ((z1, z2), (z3, z4)) = DeeplyDisturbed();
	}

	(int a, short b) Frob() => (1, 2);
	((int a, short b), (double c, char d)) DeeplyDisturbed() => throw new System.NotImplementedException();
}
