using System;
using System.Threading.Tasks;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override async Task Method1()
		{
			await base.Method1();
		}

		public override async Task<int> Method2()
		{
			return await base.Method2();
		}

		public override async Task<int> Method3()
		{
			return await base.Method3().ConfigureAwait(false);
		}
	}

	class BaseClass
	{
		public virtual Task Method1() => Task.CompletedTask;
		public virtual Task<int> Method2() => Task.FromResult(42);
		public virtual Task<int> Method3() => Task.FromResult(42);
	}
}
