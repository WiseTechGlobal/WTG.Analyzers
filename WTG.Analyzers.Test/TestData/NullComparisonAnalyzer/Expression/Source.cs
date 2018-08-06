using System;
using System.Linq.Expressions;

public static class Bob
{
	public static Expression<Func<int, bool>> Method1 = value => value == null;
	public static Expression<Func<int, bool>> Method2 = value => value != null;
}
