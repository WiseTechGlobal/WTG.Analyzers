using System.Threading.Tasks;

public static class Bob
{
	public static Task M1() => Task.FromResult(true);
	public static Task M2() => Task.FromResult<object>(null);

	public static Task<bool> N1() => Task.FromResult(true);
	public static Task<object> N2() => Task.FromResult<object>(null);
}
