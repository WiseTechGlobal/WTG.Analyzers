using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public static class Bob
{
	IEnumerable<Tuple<bool>> Enumerable(IEnumerable<Tuple<bool>> source)
	{
		return
			from tuple in source
			where tuple.Item1
			orderby !tuple.Item1
			let x = tuple.Item1
			group tuple by x into g
			select !g.Key;
	}

	IQueryable<Tuple<bool>> Queryable(IQueryable<Tuple<bool>> source)
	{
		return
			from tuple in source
			where tuple.Item1
			orderby !tuple.Item1
			let x = tuple.Item1
			group tuple by x into g
			select !g.Key;
	}
}
