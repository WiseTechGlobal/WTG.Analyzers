using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace TestAssembly
{
	public class PedanticForAll
	{
		public static string ToCsvLineOrSomething(IEnumerable<string> s)
		{
			Contract.Requires(s != null);
			Contract.Requires(Contract.ForAll(s, x => x != null));
			return string.Join(",", s);
		}
	}
}
