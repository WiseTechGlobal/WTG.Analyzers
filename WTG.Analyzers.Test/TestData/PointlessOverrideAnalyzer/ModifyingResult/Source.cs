using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override bool Method(int argument1) => !base.Method(argument1);
	}

	class BaseClass
	{
		public virtual bool Method(int argument1) => true;
	}
}
