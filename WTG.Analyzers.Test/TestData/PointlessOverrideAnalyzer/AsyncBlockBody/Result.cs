using System;
using System.Threading.Tasks;

namespace Magic
{
	class DerivedClass : BaseClass
	{
	}

	class BaseClass
	{
		public virtual Task Method1() => Task.CompletedTask;
		public virtual Task<int> Method2() => Task.FromResult(42);
		public virtual Task<int> Method3() => Task.FromResult(42);
	}
}
