using System.Collections.Generic;
using System.Linq;

public class Bob
{
	public int[] Method(Bob other)
	{
		var items = other.GetItems()?.Where(x => x > 0).Append(42).ToArray();
		return items;
	}

	public IEnumerable<int> GetItems() => new[] { 1, 2, 3 };
}
