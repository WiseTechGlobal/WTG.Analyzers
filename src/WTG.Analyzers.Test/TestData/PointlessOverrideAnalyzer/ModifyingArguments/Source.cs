using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1) => base.Method(argument1 + 1);
		public override void Method(int argument1, int argument2) => base.Method(argument1, 0);
		public override void Foo(int argument1, int argument2) => Foo(argument2, argument1);
	}

	class BaseClass
	{
		public virtual void Method(int argument1)
		{
		}

		public virtual void Method(int argument1, int argument2)
		{
		}

		public virtual void Foo(int argument1, int argument2)
		{
		}
	}
}
