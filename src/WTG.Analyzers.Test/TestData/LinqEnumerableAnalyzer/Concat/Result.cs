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

	public IEnumerable<object> Method7(object first, IEnumerable<object> other) => (other ?? Enumerable.Empty<object>()).Prepend(first);
	public IEnumerable<object> Method8(IEnumerable<object> first, object other) => (first ?? Enumerable.Empty<object>()).Append(other);

	public IEnumerable<int> Method9(IEnumerable<int> values) =>
		values
		.Append(123)
		.Distinct();

	public IEnumerable<int> Method10(IEnumerable<int> values) =>
		values
		.Prepend(123)
		.Distinct();
}
