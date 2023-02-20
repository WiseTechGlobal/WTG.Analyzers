using System.Collections.Generic;

public class Foo
{
	public IEnumerable<int> AppendThree(IEnumerable<int> source) => System.Linq.Enumerable.Concat(source, new[] { 3 });
	public IEnumerable<int> PrependThree(IEnumerable<int> source) => System.Linq.Enumerable.Concat(new[] { 3 }, source);
}
