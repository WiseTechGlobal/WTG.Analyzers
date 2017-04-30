namespace Magic
{
	class A
	{
#if DEBUG
		void MethodA1()
		{
		}
#endif

#if !DEBUG
		void MethodA2()
		{
		}
#elif DEBUG
		void MethodA2_DEBUG()
		{
		}
#endif

#if Awsome || DEBUG
		void MethodA3()
		{
		}
#endif
	}
}