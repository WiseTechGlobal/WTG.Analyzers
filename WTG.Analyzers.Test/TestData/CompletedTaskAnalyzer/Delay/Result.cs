using System.Threading.Tasks;

public static class Bob
{
	public static Task M1() => Task.CompletedTask;
	public static Task M2() => Task.CompletedTask;
	public static Task M3() => Task.Delay(1);
}
