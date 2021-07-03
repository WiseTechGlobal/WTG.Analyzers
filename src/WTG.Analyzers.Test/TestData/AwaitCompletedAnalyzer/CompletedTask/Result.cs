using System.Threading.Tasks;

public class Bob
{
	public async Task Method1()
	{
		Pre();
		await BarAsync();
		Post();
	}

	public async Task Method2()
	{
		Pre();
		Post();
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
}
