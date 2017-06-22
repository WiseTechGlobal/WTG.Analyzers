using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Bob
{
	public static async void AVMethod1()
	{
		async void V1() => await Stuff();
		async void V2() => await Stuff().ConfigureAwait(false); // async void - not ok
		async void V3() => await Stuff().ConfigureAwait(true); // async void - not ok

		async Task T1() => await Stuff();
		async Task T2() => await Stuff().ConfigureAwait(false); // async Task - ok
		async Task T3() => await Stuff().ConfigureAwait(true); // async Task - ok
	}

	public static async Task ATMethodAsync1()
	{
		async void V1() => await Stuff();
		async void V2() => await Stuff().ConfigureAwait(false); // async void - not ok
		async void V3() => await Stuff().ConfigureAwait(true); // async void - not ok

		async Task T1() => await Stuff();
		async Task T2() => await Stuff().ConfigureAwait(false); // async Task - ok
		async Task T3() => await Stuff().ConfigureAwait(true); // async Task - ok
	}

	public static async void AVMethod2()
	{
		async void V1() { await Stuff(); }
		async void V2() { await Stuff().ConfigureAwait(false); } // async void - not ok
		async void V3() { await Stuff().ConfigureAwait(true); } // async void - not ok

		async Task T1() { await Stuff(); }
		async Task T2() { await Stuff().ConfigureAwait(false); } // async Task - ok
		async Task T3() { await Stuff().ConfigureAwait(true); } // async Task - ok
	}

	public static async Task ATMethodAsync2()
	{
		async void V1() { await Stuff(); }
		async void V2() { await Stuff().ConfigureAwait(false); } // async void - not ok
		async void V3() { await Stuff().ConfigureAwait(true); } // async void - not ok

		async Task T1() { await Stuff(); }
		async Task T2() { await Stuff().ConfigureAwait(false); } // async Task - ok
		async Task T3() { await Stuff().ConfigureAwait(true); } // async Task - ok
	}

	static Task Stuff()
	{
		return Task.FromResult<object>(null);
	}
}
