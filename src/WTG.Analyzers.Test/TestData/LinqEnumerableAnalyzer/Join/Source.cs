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

		// TESTING prepend WTG3014

		new[] { 0 }.Concat(new[] { 1 });

		a.Concat(b); // there is no flow analysis to guarantee that the single element collection remains single element
		Dictionary<int, int> dict = new Dictionary<int, int>() { { 2, 1 } };
		dict.Concat(new Dictionary<int, int>() {
			{1, 1}
		}); // analyzer does not check dictionaries despite them using ObjectCreationExpression
	}
}
