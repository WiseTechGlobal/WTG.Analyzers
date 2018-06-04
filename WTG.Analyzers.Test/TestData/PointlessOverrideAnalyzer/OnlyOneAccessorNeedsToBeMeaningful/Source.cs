using System;

namespace Magic
{
	class DerivedClass1 : BaseClass
	{
		public override int Property
		{
			get => base.Property + 1;
			set => base.Property = value;
		}

		public override int this[int index]
		{
			get => base[index] + 1;
			set => base[index] = value;
		}

		public override event EventHandler Event
		{
			add { }
			remove => base.Event -= value;
		}
	}

	class DerivedClass2 : BaseClass
	{
		public override int Property
		{
			get => base.Property;
			set => base.Property = value + 1;
		}

		public override int this[int index]
		{
			get => base[index];
			set => base[index] = value + 1;
		}

		public override event EventHandler Event
		{
			add => base.Event += value;
			remove { }
		}
	}

	class BaseClass
	{
		public virtual int Property { get; set; }

		public virtual int this[int index]
		{
			get { return 42; }
			set { }
		}

		public virtual event EventHandler Event
		{
			add { }
			remove { }
		}
	}
}
