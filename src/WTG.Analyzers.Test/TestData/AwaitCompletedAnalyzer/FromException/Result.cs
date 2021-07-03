using System;
using System.Threading.Tasks;

public class Bob
{
	public async Task<int> Method1()
	{
		Pre();
		await BarAsync();
		throw new InvalidOperationException("foo");
		Post();
		return 42;
	}

	public async Task<int> Method2()
	{
		Pre();
		await BarAsync();
		throw new InvalidOperationException("bar");
	}

	public async Task Method3()
	{
		Pre();

		DoStuff(
			MethodWithSideEffects(),
			throw new InvalidOperationException("baz"),
			MethodWithSideEffects());

		Post();
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
	object MethodWithSideEffects() => null;
	void DoStuff(object a, object b, object c) { }
}
