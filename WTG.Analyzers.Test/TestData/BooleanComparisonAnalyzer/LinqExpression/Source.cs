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
			where tuple.Item1 == true
			orderby tuple.Item1 == false
			let x = tuple.Item1 == true
			group tuple by x == true into g
			select g.Key == false;
	}

	IQueryable<Tuple<bool>> Queryable(IQueryable<Tuple<bool>> source)
	{
		return
			from tuple in source
			where tuple.Item1 == true
			orderby tuple.Item1 == false
			let x = tuple.Item1 == true
			group tuple by x == true into g
			select g.Key == false;
	}
}
