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
		a.Concat(b);

		new Dictionary<int, int>() { { 2, 1 } }.Concat(new Dictionary<int, int>() {
			{1, 1}
		});
	}

	public IEnumerable<int> Method1() => new[] { 1 }.Concat(new[] { 2 });
	public IEnumerable<int> Method2() => new int[] { 1 }.Concat(new[] { 2 });
	public IEnumerable<int> Method3() => new List<int>() { 1 }.Concat(new[] { 2 });

	public IEnumerable<int> Method4() => (new[] { 1 }).Concat(new[] { 2 });
	public IEnumerable<int> Method5() => (new int[] { 1 }).Concat(new[] { 2 });
	public IEnumerable<int> Method6() => (new List<int>() { 1 }).Concat(new[] { 2 });

	public IEnumerable<object> Method7(object first, IEnumerable<object> other) => new[] { first }.Concat(other ?? Enumerable.Empty<object>());
	public IEnumerable<object> Method8(IEnumerable<object> first, object other) => (first ?? Enumerable.Empty<object>()).Concat(new[] { other });

	public IEnumerable<int> Method9(IEnumerable<int> values) =>
		values
		.Concat(new[] { 123 })
		.Distinct();

	public IEnumerable<int> Method10(IEnumerable<int> values) =>
		new[] { 123 }
		.Concat(values)
		.Distinct();

	public IEnumerable<int> Method11(IEnumerable<int> source)
		=> source.Concat(new List<int>() { [0] = 4 });

	public IEnumerable<int> Method12(IEnumerable<int> source)
		=> source.Concat(new List<int>() { { 4 } });

	public IEnumerable<int> Method13()
		=> new[] { 1, 2, 3 }.Concat(new[] { 4 });

	public IEnumerable<int> Method14()
		=> new int[] { 1, 2, 3 }.Concat(new[] { 4 });

	public IEnumerable<int> Method15()
		=> new List<int>() { 1, 2, 3 }.Concat(new[] { 4 });

	public IEnumerable<int> Method16()
		=> new[] { 1 }.Concat(new[] { 2, 3, 4 });

	public IEnumerable<int> Method17()
		=> new int[] { 1 }.Concat(new[] { 2, 3, 4 });

	public IEnumerable<int> Method18()
		=> new List<int>() { 1 }.Concat(new[] { 2, 3, 4 });

	public IEnumerable<int> Method19()
		=> new[] { 1, 2, 3 }.Concat(new[] { 4, 5, 6 });

	public IEnumerable<int> Method20()
		=> new int[] { 1, 2, 3 }.Concat(new[] { 4, 5, 6 });

	public IEnumerable<int> Method21()
		=> new List<int>() { 1, 2, 3 }.Concat(new[] { 4, 5, 6 });
}
