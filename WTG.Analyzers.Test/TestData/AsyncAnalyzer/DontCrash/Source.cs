using System.Threading.Tasks;

public static class Bob
{
	public static async void ATMethodAsync(MissingTypeInfo info)
	{
		await info.ConfigureAwait(false);
	}

	public static void UnknownDelegateType(Task task)
	{
		Foo foo = async () => await task.ConfigureAwait(false);
	}
}
