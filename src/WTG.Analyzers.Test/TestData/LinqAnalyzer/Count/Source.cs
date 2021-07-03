using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class Bob
{
	public int Method(string source) => source.Count();
	public int Method(int[] source) => source.Count();
	public int Method(IReadOnlyList<int> source) => source.Count();
	public int Method(IReadOnlyCollection<int> source) => source.Count();
	public int Method(IList<int> source) => source.Count();
	public int Method(ICollection<int> source) => source.Count();
	public int Method(IEnumerable<int> source) => source.Count();

	public int MethodA(int[] source) => Enumerable.Count(source);
	public Expression<Func<int[], int>> Query = source => source.Count();

	public int? Method1(int[] source) => source?.Count(); // don't suggest for conditional member access (yet).
	public int Method2(ExplicitCollection<int> source) => source.Count(); // don't suggest if using the property would require casting.
	public int Method3(UnknownCollection source) => source.Count(); // don't suggest if we don't recognise the type.
	public int Method4(int[] source) => source.Count(x => x > 0); // don't suggest if a filter is provided.
}

public struct ExplicitCollection<T> : IReadOnlyCollection<T>
{
	public IEnumerator<T> GetEnumerator()
	{
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	int IReadOnlyCollection<T>.Count => 0;
}
