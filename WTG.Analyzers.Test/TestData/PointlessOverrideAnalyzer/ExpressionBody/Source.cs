using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1, int argument2) => base.Method(argument1, argument2);

		public override int Property
		{
			get => base.Property;
			set => base.Property = value;
		}

		public override int ReadOnlyProperty => base.ReadOnlyProperty;

		public override int this[int index]
		{
			get => base[index];
			set => base[index] = value;
		}

		public override event EventHandler Event
		{
			add => base.Event += value;
			remove => base.Event -= value;
		}
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
