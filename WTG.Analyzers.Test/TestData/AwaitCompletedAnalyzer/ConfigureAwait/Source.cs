using System.Threading.Tasks;

public class Bob
{
	public async Task Method1()
	{
		Pre();
		await BarAsync().ConfigureAwait(false);
		await Task.CompletedTask.ConfigureAwait(false);
		Post();
	}

	public async Task<int> Method2()
	{
		Pre();
		await BarAsync().ConfigureAwait(false);
		var result = await Task.FromResult(42).ConfigureAwait(false);
		Post();
		return result;
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
}
