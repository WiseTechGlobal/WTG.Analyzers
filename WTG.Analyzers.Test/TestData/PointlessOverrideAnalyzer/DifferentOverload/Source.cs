using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1) => base.Method(argument1, 42);
		public override void Method(int argument1, int argument2) => base.Method(argument1);
	}

	class BaseClass
	{
		public virtual void Method(int argument1)
		{
		}

		public virtual void Method(int argument1, int argument2)
		{
		}
	}
}
