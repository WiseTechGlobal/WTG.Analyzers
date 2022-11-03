using System;

namespace Magic
{
	class A
	{
		public void Array()
		{
			var bob = new[]
			{
				"A"
#if true
				,"B"
#endif
				,"C"
			};
		}

		public string Switch(object item)
		{
			return item switch
			{
				null => "<null>"
#if true
				,int x => x.ToString()
#endif
				,object o => o.ToString()
			};
		}

		enum ConditionalEnum
		{
			A
#if true
			,B
#endif
			,C
		}

		public string Parameter()
		{
			return string.Format(
				"text"
				,1
#if true
				,2
				,3
				,4
#endif
				,5);
		}
	}
}
