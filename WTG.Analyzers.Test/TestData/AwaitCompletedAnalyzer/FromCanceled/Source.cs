using System;
using System.Threading;
using System.Threading.Tasks;

public class Bob
{
	public async Task<int> Method1(CancellationToken cancellationToken)
	{
		Pre();
		await BarAsync();
		await Task.FromCanceled(cancellationToken);
		Post();
		return 42;
	}

	public async Task<int> Method2(CancellationToken cancellationToken)
	{
		Pre();
		await BarAsync();
		return await Task.FromCanceled<int>(cancellationToken);
	}

	public async Task Method3(CancellationToken cancellationToken)
	{
		Pre();

		DoStuff(
			MethodWithSideEffects(),
			await Task.FromCanceled<object>(cancellationToken),
			MethodWithSideEffects());

		Post();
	}

	void Pre() { }
	void Post() { }
	Task BarAsync() => Task.CompletedTask;
	object MethodWithSideEffects() => null;
	void DoStuff(object a, object b, object c) { }
}
