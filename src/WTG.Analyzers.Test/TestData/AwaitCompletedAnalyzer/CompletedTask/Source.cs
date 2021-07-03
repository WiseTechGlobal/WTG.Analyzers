using System.Threading.Tasks;

public class Bob
{
	public async Task Method1()
	{
		Pre();
		await BarAsync();
		await Task.CompletedTask;
		Post();
	}

	public async Task Method2()
	{
		Pre();
		await Task.CompletedTask;
		Post();
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
}
