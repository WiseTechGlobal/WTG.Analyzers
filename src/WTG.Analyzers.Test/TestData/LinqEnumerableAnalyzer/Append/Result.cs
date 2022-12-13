using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bob
{
	public void Method()
	{
		IEnumerable<int> a = Enumerable.Empty<int>();
		int[] b = new[] { 1 };
		int j = 5;

		a.Append(4);
		a.Append(4);
		a.Append(4);
		a.Append(j);
		a.Append(j);
		a.Append(j);

		Enumerable.Append(a, 4);
		Enumerable.Append(a, 4);
		Enumerable.Append(a, 4);
		Enumerable.Append(a, j);
		Enumerable.Append(a, j);
		Enumerable.Append(a, j);

		a.Concat(b);
		Dictionary<int, int> dict = new Dictionary<int, int>() { { 2, 1 } };
		dict.Concat(new Dictionary<int, int>() {
			{1, 1}
		});
	}
}
