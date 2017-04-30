using System;

namespace Magic
{
	class A
	{
#if !Alpha
		public
#endif
		void MethodA1()
		{
		}

#if Alpha
#elif !Beta
		public
#else
#endif
		void MethodA3()
		{

		}

#if Alpha
#else
		public
#endif
		void MethodA4()
		{

		}

#if Alpha
#elif Beta
#elif !Gamma
		public
#endif
		void MethodA4()
		{

		}
	}
}