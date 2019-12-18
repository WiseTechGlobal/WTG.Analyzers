using System.Threading.Tasks;

public class Bob
{
	public async Task Method1()
	{
		Pre();
		await BarAsync().ConfigureAwait(false);
		Post();
	}

	public async Task<int> Method2()
	{
		Pre();
		await BarAsync().ConfigureAwait(false);
		var result = 42;
		Post();
		return result;
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
}
