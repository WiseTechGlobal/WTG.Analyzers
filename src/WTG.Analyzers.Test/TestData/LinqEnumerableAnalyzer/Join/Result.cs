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

		a.Concat(b); // there is no flow analysis to guarantee that the single element collection remains single element
		new Dictionary<int, int>() { { 2, 1 } }.Concat(new Dictionary<int, int>() {
			{1, 1}
		}); // analyzer does not check dictionaries despite them using ObjectCreationExpression
	}

	public IEnumerable<int> Method1() => new[] { 1, 2 };
	public IEnumerable<int> Method2() => new[] { 1, 2 };
	public IEnumerable<int> Method3() => new[] { 1, 2 };

	public IEnumerable<int> Method4() => new[] { 1, 2 };
	public IEnumerable<int> Method5() => new[] { 1, 2 };
	public IEnumerable<int> Method6() => new[] { 1, 2 };
}
