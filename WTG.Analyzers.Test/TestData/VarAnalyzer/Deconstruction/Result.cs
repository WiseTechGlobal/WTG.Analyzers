public class Bob
{
	void WellKnown()
	{
		(var x1, var x2) = Frob();
		(var y1, var y2) = (2, 3);
		((var z1, var z2), (var z3, var z4)) = DeeplyDisturbed();
	}

	void NotWellKnown()
	{
		(var x1, int x2) = Frob();
		(var y1, short y2) = (2, 3);
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
