using System;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Foo()();
		}

		static Action Foo() => null;
	}
}
