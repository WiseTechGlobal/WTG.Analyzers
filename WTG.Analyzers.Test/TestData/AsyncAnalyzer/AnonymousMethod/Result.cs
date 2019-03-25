using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Bob
{
	public static async void AVMethod()
	{
		Munch(async delegate { await Stuff(); });
		Munch(async delegate { await Stuff(); }); // async void - not ok
		Munch(async delegate { await Stuff().ConfigureAwait(true); });
	}

	public static async Task ATMethodAsync()
	{
		AsyncMunch(async delegate { await Stuff(); });
		AsyncMunch(async delegate { await Stuff().ConfigureAwait(false); }); // async Task - ok
		AsyncMunch(async delegate { await Stuff().ConfigureAwait(true); });
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
