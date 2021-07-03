using System;

namespace TestAssembly
{
	class CodeAnalysisSuppression
	{
		public bool Foo(DateTime x) => x.DayOfWeek == DayOfWeek.Sunday;


		public bool Bar(DateTime x) => x.Day != 2;
	}
}
