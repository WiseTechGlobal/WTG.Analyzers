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
			where false
			orderby true
			let x = false
			group tuple by true into g
			select false;
	}

	public static IQueryable<bool> Queryable(IQueryable<Tuple<Guid>> source)
	{
		return
			from tuple in source
			where false
			orderby true
			let x = false
			group tuple by true into g
			select false;
	}
}
