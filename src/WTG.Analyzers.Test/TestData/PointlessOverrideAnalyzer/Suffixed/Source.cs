using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1, int argument2)
		{
			base.Method(argument1, argument2);
			Foo();
		}

		public override int Property
		{
			get
			{
				return base.Property;
				Foo();
			}
			set
			{
				base.Property = value;
				Foo();
			}
		}

		public override int ReadOnlyProperty
		{
			get
			{
				return base.ReadOnlyProperty;
				Foo();
			}
		}

		public override int this[int index]
		{
			get
			{
				return base[index];
				Foo();
			}
			set
			{
				base[index] = value;
				Foo();
			}
		}

		public override event EventHandler Event
		{
			add
			{
				base.Event += value;
				Foo();
			}
			remove
			{
				base.Event -= value;
				Foo();
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
