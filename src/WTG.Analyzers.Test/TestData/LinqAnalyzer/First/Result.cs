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

	public int MethodA(int[] source) => source[0];
	public int MethodB(int[][] source) => source[0][0];
	public Expression<Func<int[], int>> Query = source => source[0];

	public int? Method1(int[] source) => source?.First(); // don't suggest for conditional member access (yet).
	public int Method2(ExplicitCollection<int> source) => source.First(); // don't suggest if using the indexer would require casting.
	public int Method3(UnknownCollection source) => source.First(); // don't suggest if we don't recognise the type.
	public int Method4(int[] source) => source.First(x => x > 0); // don't suggest if a filter is provided.
}

public struct ExplicitCollection<T> : IReadOnlyList<T>
{
	public IEnumerator<T> GetEnumerator()
	{
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	T IReadOnlyList<T>.this[int index] => throw new ArgumentOutOfRangeException();
	int IReadOnlyCollection<T>.Count => 0;
}
