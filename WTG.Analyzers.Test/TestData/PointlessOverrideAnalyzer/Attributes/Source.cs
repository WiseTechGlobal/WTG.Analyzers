using System;
using System.ComponentModel;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		[Obsolete("Please don't use this anymore")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Method(int argument1) => base.Method(argument1);
	}

	class BaseClass
	{
		public virtual void Method(int argument1)
		{
		}
	}
}
