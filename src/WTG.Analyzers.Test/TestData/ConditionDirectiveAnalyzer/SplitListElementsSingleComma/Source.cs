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
				,
#endif
				"B"
			};
		}

		public string Switch(object item)
		{
			return item switch
			{
				null => "<null>"
#if true
				,
#endif
				object o => o.ToString(),
			};
		}

		enum ConditionalEnum
		{
			A
#if true
			,
#endif
			C,
		}

		public string Parameter()
		{
			return string.Format(
				"text",
				1
#if true
				,
#endif
				2);
		}
	}
}
