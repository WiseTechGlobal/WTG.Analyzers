using System.Collections.Generic;

public class Foo
{
	public IEnumerable<int> AppendThree(IEnumerable<int> source) => System.Linq.Enumerable.Append(source, 3);
	public IEnumerable<int> PrependThree(IEnumerable<int> source) => System.Linq.Enumerable.Prepend(source, 3);
}
