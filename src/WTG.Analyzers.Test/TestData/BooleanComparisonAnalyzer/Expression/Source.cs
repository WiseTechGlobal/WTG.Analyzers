using System;
using System.Linq.Expressions;

public static class Bob
{
	public static Expression<Func<bool, bool>> Method1 = value => value == true;
	public static Expression<Func<bool, bool>> Method2 = value => value != true;
}
