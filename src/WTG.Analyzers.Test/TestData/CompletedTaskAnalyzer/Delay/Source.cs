using System.Threading.Tasks;

public static class Bob
{
	public static Task M1() => Task.Delay(0);
	public static Task M2() => Task.Delay(0x0);
	public static Task M3() => Task.Delay(1);
}
