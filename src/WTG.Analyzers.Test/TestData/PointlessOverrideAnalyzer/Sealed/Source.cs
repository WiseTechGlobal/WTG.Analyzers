using System;

namespace Magic
{
	class SealedOverrideClass : BaseClass
	{
		public sealed override void Method(int argument1, int argument2)
		{
			base.Method(argument1, argument2);
			Foo();
		}

		public sealed override int Property
		{
			get
			{
				return base.Property;
			}
			set
			{
				base.Property = value;
			}
		}

		public sealed override int ReadOnlyProperty
		{
			get
			{
				return base.ReadOnlyProperty;
			}
		}

		public sealed override int this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		public sealed override event EventHandler Event
		{
			add
			{
				base.Event += value;
			}
			remove
			{
				base.Event -= value;
			}
		}

		void Foo()
		{
		}
	}

	class OverrideSealedClass : BaseClass
	{
		public override sealed void Method(int argument1, int argument2)
		{
			base.Method(argument1, argument2);
			Foo();
		}

		public override sealed int Property
		{
			get
			{
				return base.Property;
			}
			set
			{
				base.Property = value;
			}
		}

		public override sealed int ReadOnlyProperty
		{
			get
			{
				return base.ReadOnlyProperty;
			}
		}

		public override sealed int this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		public override sealed event EventHandler Event
		{
			add
			{
				base.Event += value;
			}
			remove
			{
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
