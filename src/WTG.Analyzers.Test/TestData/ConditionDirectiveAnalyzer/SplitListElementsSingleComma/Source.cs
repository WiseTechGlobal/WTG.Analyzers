using System;

namespace Magic
{
	class A
	{
		public void Array()
		{
			var bob = new[]
			{
				123
#if true
				,
#endif
				-123
			};
		}

		public string Parameter()
		{
			return string.Format(
				"text",
				1
#if true
				,
#endif
				-1);
		}
	}
}
