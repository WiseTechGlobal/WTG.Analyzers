using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1, int argument2)
		{
			Foo();
			base.Method(argument1, argument2);
		}

		public override int Property
		{
			get
			{
				Foo();
				return base.Property;
			}
			set
			{
				Foo();
				base.Property = value;
			}
		}

		public override int ReadOnlyProperty
		{
			get
			{
				Foo();
				return base.ReadOnlyProperty;
			}
		}

		public override int this[int index]
		{
			get
			{
				Foo();
				return base[index];
			}
			set
			{
				Foo();
				base[index] = value;
			}
		}

		public override event EventHandler Event
		{
			add
			{
				Foo();
				base.Event += value;
			}
			remove
			{
				Foo();
				base.Event -= value;
			}
		}

		void Foo()
		{
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
