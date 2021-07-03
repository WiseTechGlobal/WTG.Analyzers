using System.Threading.Tasks;

public static class Bob
{
	public static void M1() => X(Task.CompletedTask);
	public static void M2() => X(Task.CompletedTask);

	public static void N1() => Y(Task.FromResult(true));
	public static void N2() => Y(Task.FromResult<object>(null));

	static void X(Task task) { }
	static void Y<T>(Task<T> task) { }
}
