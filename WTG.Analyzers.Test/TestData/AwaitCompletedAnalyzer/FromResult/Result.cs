using System.Threading.Tasks;

public class Bob
{
	public async Task<int> Method1()
	{
		Pre();
		await BarAsync();
		var value = 42;
		Post();
		return value;
	}

	public async Task<int> Method2()
	{
		Pre();
		await BarAsync();
		return 42;
	}

	public async Task<int> Method3()
	{
		Pre();
		var value = 42;
		Post();
		return value;
	}

	public async Task<int> Method4()
	{
		Pre();
		return 42;
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
}
