public static class Bob
{
	public static System.Threading.Tasks.Task M1() => System.Threading.Tasks.Task.Delay(0);
	public static System.Threading.Tasks.Task M2() => System.Threading.Tasks.Task.Delay(0x0);
	public static System.Threading.Tasks.Task M3() => System.Threading.Tasks.Task.Delay(1);
}
