using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public static class Bob
{
	public static IEnumerable<bool> Enumerable(IEnumerable<Tuple<Guid>> source)
	{
		return
			from tuple in source
			where tuple.Item1 == null
			orderby tuple.Item1 != null
			let x = tuple.Item1 == null
			group tuple by x != null into g
			select g.Key == null;
	}

	public static IQueryable<bool> Queryable(IQueryable<Tuple<Guid>> source)
	{
		return
			from tuple in source
			where tuple.Item1 == null
			orderby tuple.Item1 != null
			let x = tuple.Item1 == null
			group tuple by x != null into g
			select g.Key == null;
	}
}
