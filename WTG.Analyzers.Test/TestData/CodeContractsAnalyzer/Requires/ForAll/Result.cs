using System.Collections.Generic;

namespace TestAssembly
{
	public class PedanticForAll
	{
		public static string ToCsvLineOrSomething(IEnumerable<string> s)
		{
			if (s == null)
			{
				throw new System.ArgumentNullException(nameof(s));
			}

			return string.Join(",", s);
		}
	}
}
