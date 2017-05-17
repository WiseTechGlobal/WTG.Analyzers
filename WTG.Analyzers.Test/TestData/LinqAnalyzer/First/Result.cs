using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class Bob
{
	public int Method(string source) => source[0];
	public int Method(int[] source) => source[0];
	public int Method(IReadOnlyList<int> source) => source[0];
	public int Method(IReadOnlyCollection<int> source) => source.First();
	public int Method(IList<int> source) => source[0];
	public int Method(ICollection<int> source) => source.First();
	public int Method(IEnumerable<int> source) => source.First();

	public int Method(int[] source) => source[0];
	public int Method(int[][] source) => source[0][0];
	public Expression<Func<int[], int>> Query = source => source[0];

	public int? Method(int[] source) => source?.First(); // don't suggest for conditional member access (yet).
	public int Method(ExplicitCollection<int> source) => source.First(); // don't suggest if using the indexer would require casting.
	public int Method(UnknownCollection source) => source.First(); // don't suggest if we don't recognise the type.
	public int Method(int[] source) => source.First(x => x > 0); // don't suggest if a filter is provided.
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
