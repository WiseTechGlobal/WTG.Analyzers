using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Bob
{
	public static async void AVMethod()
	{
		Munch(async () => await Stuff());
		Munch(async () => await Stuff()); // async void - not ok
		Munch(async () => await Stuff()); // async void - not ok
	}

	public static async Task ATMethodAsync()
	{
		AsyncMunch(async () => await Stuff());
		AsyncMunch(async () => await Stuff().ConfigureAwait(false)); // async Task - ok
		AsyncMunch(async () => await Stuff().ConfigureAwait(true)); // async Task - ok
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
