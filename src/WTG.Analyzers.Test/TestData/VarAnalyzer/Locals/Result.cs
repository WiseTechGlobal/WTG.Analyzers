using System.Collections.Generic;
using System.Linq;

public class Bob
{
	public void Method(int value)
	{
		int unvarableLocal; // can't translate this so don't suggest it.
		var varableLocal = value;
		object wrongType = value; // can't translate this, so don't suggest it.
		const int Value = 42; // can't use var on constants, so don't suggest it.
		unvarableLocal = value;
	}

	public void List(IEnumerable<int> values)
	{
		var list = values.ToList();
	}
}
