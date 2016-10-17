using System;

public class Bob
{
	public void Bob()
	{
		// comment
		Barry();
	}

	void Barry()
	{
		var bob = from a in new int[] { }
				  where a > 10
				  select a;
	}
}
