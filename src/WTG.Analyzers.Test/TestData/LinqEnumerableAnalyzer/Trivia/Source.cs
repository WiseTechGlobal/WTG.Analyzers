using System.Collections.Generic;
using System.Linq;

public class Foo
{
	void Method(IEnumerable<int> list)
	{
		var prepended = new[] { 42 } // comment 1
			.Concat(list) // comment 2
			.Select(x => x);

		var appended = list // comment 3
			.Concat(new[] { 42 }) // comment 4
			.Select(x => x);
	}
}
