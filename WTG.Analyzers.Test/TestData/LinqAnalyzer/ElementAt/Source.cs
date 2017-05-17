using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class Bob
{
	public int Method(string source) => source.ElementAt(5);
	public int Method(int[] source) => source.ElementAt(5);
	public int Method(IReadOnlyList<int> source) => source.ElementAt(5);
	public int Method(IReadOnlyCollection<int> source) => source.ElementAt(5);
	public int Method(IList<int> source) => source.ElementAt(5);
	public int Method(ICollection<int> source) => source.ElementAt(5);
	public int Method(IEnumerable<int> source) => source.ElementAt(5);

	public int Method(int[] source) => Enumerable.ElementAt(source, 5);
	public int Method(int[][] source) => source.ElementAt(5).ElementAt(4);
	public Expression<Func<int[], int>> Query = source => source.ElementAt(5);

	public int? Method(int[] source) => source?.ElementAt(5); // don't suggest for conditional member access (yet).
	public int Method(ExplicitCollection<int> source) => source.ElementAt(5); // don't suggest if using the indexer would require casting.
	public int Method(UnknownCollection source) => source.ElementAt(5); // don't suggest if we don't recognise the type.
}

struct ExplicitCollection<T> : IReadOnlyList<T>
{
	public IEnumerator<T> GetEnumerator()
	{
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	int IReadOnlyList<T>.this[int index] => throw new ArgumentOutOfRangeException();
	int IReadOnlyCollection<T>.Count => Length;
}
