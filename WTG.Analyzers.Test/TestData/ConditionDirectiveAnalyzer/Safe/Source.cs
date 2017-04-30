namespace Magic
{
	class A
	{
		void MethodA1()
		{
		}

		void MethodA2()
		{
#if Magic
			Method1A();
#endif
		}

#if !Bob
		void MethodB1()
		{
#if Magic
			Method1A();
#endif
		}

		void MethodB2()
		{
		}
#endif
	}
}