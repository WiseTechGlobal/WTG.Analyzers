using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1, int argument2) => target.Method(argument1, argument2);

		public override int Property
		{
			get => target.Property;
			set => target.Property = value;
		}

		public override int ReadOnlyProperty => target.ReadOnlyProperty;

		public override int this[int index]
		{
			get => target[index];
			set => target[index] = value;
		}

		public override event EventHandler Event
		{
			add => target.Event += value;
			remove => target.Event -= value;
		}

		BaseClass target;
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
