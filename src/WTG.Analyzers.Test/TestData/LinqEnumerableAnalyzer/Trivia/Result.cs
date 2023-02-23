using System.Collections.Generic;
using System.Linq;

public class Foo
{
	void Method(IEnumerable<int> list)
	{
		var prepended = list // comment 1
			.Prepend(42) // comment 2
			.Select(x => x);

		var appended = list // comment 3
			.Append(42) // comment 4
			.Select(x => x);
	}
}
