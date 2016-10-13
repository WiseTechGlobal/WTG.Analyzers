public static class Bob
{
	public static async void ATMethodAsync(MissingTypeInfo info)
	{
		await info.ConfigureAwait(false);
	}
}
