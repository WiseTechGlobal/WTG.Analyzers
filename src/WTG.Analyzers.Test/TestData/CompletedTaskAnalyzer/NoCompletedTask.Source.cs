using System.Threading.Tasks;

public static class Foo
{
	public static Task Method() => Task.Delay(0);
}

namespace System.Threading.Tasks
{
	public class Task
	{
		public static Task Delay(int arg) => null;
	}
}
