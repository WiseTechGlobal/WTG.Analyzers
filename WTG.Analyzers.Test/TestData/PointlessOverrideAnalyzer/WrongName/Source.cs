using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public DerivedClass(int argument)
			: base(argument)
		{
		}

		public override void Method(int argument1, int argument2)
		{
			base.MethodX(argument1, argument2);
		}

		public override int Property
		{
			get { return base.PropertyX; }
			set { base.PropertyX = value; }
		}

		public override int ReadOnlyProperty
		{
			get { return base.ReadOnlyPropertyX; }
		}

		public override event EventHandler Event
		{
			add { base.EventX += value; }
			remove { base.EventX -= value; }
		}
	}

	class BaseClass
	{
		public BaseClass(int argument)
		{
		}

		public virtual void Method(int argument1, int argument2)
		{
		}

		public virtual void MethodX(int argument1, int argument2)
		{
		}

		public virtual event EventHandler Event;
		public virtual event EventHandler EventX;

		public virtual int Property { get; set; }
		public virtual int PropertyX { get; set; }
		public virtual int ReadOnlyProperty => 42;
		public virtual int ReadOnlyPropertyX => 42;
	}
}
