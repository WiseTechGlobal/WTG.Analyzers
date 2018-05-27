using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
	}

	class BaseClass
	{
		public virtual void Method(int argument1, int argument2)
		{
		}

		public virtual event EventHandler Event
		{
			add { }
			remove { }
		}

		public virtual int Property { get; set; }
		public virtual int ReadOnlyProperty => 42;

		public virtual int this[int index]
		{
			get { return 42; }
			set { }
		}
	}
}
