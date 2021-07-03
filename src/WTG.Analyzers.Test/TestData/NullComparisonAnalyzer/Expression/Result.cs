using System;
using System.Linq.Expressions;

public static class Bob
{
	public static Expression<Func<int, bool>> Method1 = value => false;
	public static Expression<Func<int, bool>> Method2 = value => true;
}
