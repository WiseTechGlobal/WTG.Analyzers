using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Bob
{
	public static async void AVMethod()
	{
		Munch(async delegate { return await Stuff(); });
		Munch(async delegate { return await Stuff().ConfigureAwait(false); }); // async void - not ok
		Munch(async delegate { return await Stuff().ConfigureAwait(true); }); // async void - not ok
	}

	public static async Task ATMethodAsync()
	{
		AsyncMunch(async delegate { return await Stuff(); });
		AsyncMunch(async delegate { return await Stuff().ConfigureAwait(false); }); // async Task - ok
		AsyncMunch(async delegate { return await Stuff().ConfigureAwait(true); }); // async Task - ok
	}

	static Task Stuff()
	{
		return Task.FromResult<object>(null);
	}

	static void Munch(Action argument)
	{
	}

	static void AsyncMunch(Func<Task> argument)
	{
	}
}
