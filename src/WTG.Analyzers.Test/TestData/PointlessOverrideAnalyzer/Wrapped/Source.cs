using System;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		public override void Method(int argument1, int argument2)
		{
			using (Foo())
			{
				base.Method(argument1, argument2);
			}
		}

		public override int Property
		{
			get
			{
				using (Foo())
				{
					return base.Property;
				}
			}
			set
			{
				using (Foo())
				{
					base.Property = value;
				}
			}
		}

		public override int ReadOnlyProperty
		{
			get
			{
				using (Foo())
				{
					return base.ReadOnlyProperty;
				}
			}
		}

		public override int this[int index]
		{
			get
			{
				using (Foo())
				{
					return base[index];
				}
			}
			set
			{
				using (Foo())
				{
					base[index] = value;
				}
			}
		}

		public override event EventHandler Event
		{
			add
			{
				using (Foo())
				{
					base.Event += value;
				}
			}
			remove
			{
				using (Foo())
				{
					base.Event -= value;
				}
			}
		}

		IDisposable Foo() => default(IDisposable);
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
