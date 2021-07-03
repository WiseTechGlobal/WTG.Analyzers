using System;

namespace Magic
{
	class A
	{
		public void MethodA1()
		{
#if !Bob
			MethodA2();
		}

		void MethodA2()
		{
#endif
			Console.WriteLine("A?");
		}
	}
}