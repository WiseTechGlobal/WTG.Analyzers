using System;
using System.ComponentModel;

namespace Magic
{
	class DerivedClass : BaseClass
	{
		[Obsolete("Please don't use this anymore")]
		public override void Method(int argument1) => base.Method(argument1);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override object Property
		{
			get => base.Property;
			set => base.Property = value;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override event EventHandler Event
		{
			add => base.Event += value;
			remove => base.Event -= value;
		}

		[Obsolete("Please don't use this anymore")]
		public override object this[int index]
		{
			get => base[index];
			set => base[index] = value;
		}
	}

	class BaseClass
	{
		public virtual void Method(int argument1)
		{
		}

		public virtual object Property { get; set; }

		public virtual event EventHandler Event
		{
			add { }
			remove { }
		}

		public virtual object this[int index]
		{
			get => null;
			set { }
		}
	}
}
