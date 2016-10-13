using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Bob
{
	public static async void AVMethod()
	{
		await Stuff();
		await Stuff(); // async void - not ok
		await Stuff(); // async void - not ok
	}

	public static async Task ATMethodAsync()
	{
		await Stuff();
		await Stuff().ConfigureAwait(false); // async Task - ok
		await Stuff().ConfigureAwait(true); // async Task - ok
	}

	static Task Stuff()
	{
		return Task.FromResult<object>(null);
	}
}
