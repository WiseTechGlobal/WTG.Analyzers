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

		new[] { 0 }.Concat(a);
		(((new[] { 0 }))).Concat(a);
		(new int[] { 0 }).Concat(a);
		new int[] { 0 }.Concat(a);
		(new List<int>() { 0 }).Concat(a);
		new List<int>() { 0 }.Concat(a);
		new[] { j }.Concat(a);
		(new[] { j }).Concat(a);
		(new int[] { j }).Concat(a);
		new int[] { j }.Concat(a);
		(new List<int>() { j }).Concat(a);
		new List<int>() { j }.Concat(a);

		Enumerable.Concat(new[] { 0 }, a);
		Enumerable.Concat(new int[] { 0 }, a);
		Enumerable.Concat(new List<int>() { 0 }, a);
		Enumerable.Concat(new[] { j }, a);
		Enumerable.Concat(new int[] { j }, a);
		Enumerable.Concat(new List<int>() { j }, a);

        	new [] { 1 }.Concat((1 == 1) ? new [] { 1 } : new [] { 2 });

		a.Concat(b); // there is no flow analysis to guarantee that the single element collection remains single element
		Dictionary<int, int> dict = new Dictionary<int, int>() { { 2, 1 } };
		dict.Concat(new Dictionary<int, int>() {
			{1, 1}
		}); // analyzer does not check dictionaries despite them using ObjectCreationExpression
	}
}
