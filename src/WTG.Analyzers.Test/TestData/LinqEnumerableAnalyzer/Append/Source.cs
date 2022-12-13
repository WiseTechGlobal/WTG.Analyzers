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

		a.Concat(new[] { 4 });
		a.Concat(new int[] { 4 });
		a.Concat(new List<int>(){ 4 });
		a.Concat(new[] { j });
		a.Concat(new int[] { j });
		a.Concat(new List<int>() { j });

		Enumerable.Concat(a, new[] { 4 });
		Enumerable.Concat(a, new int[] { 4 });
		Enumerable.Concat(a, new List<int>() { 4 });
		Enumerable.Concat(a, new[] { j });
		Enumerable.Concat(a, new int[] { j });
		Enumerable.Concat(a, new List<int>() { j });

		a.Concat(b);
		Dictionary<int, int> dict = new Dictionary<int, int>() { { 2, 1 } };
		dict.Concat(new Dictionary<int, int>() {
			{1, 1}
		});
	}
}
