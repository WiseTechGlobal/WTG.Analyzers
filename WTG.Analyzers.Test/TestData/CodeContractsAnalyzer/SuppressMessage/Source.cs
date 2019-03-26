using System;
using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Requires-31-375", Target = "Foo.Bar.Baz")]

namespace TestAssembly
{
	class CodeAnalysisSuppression
	{
		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		public bool Foo(DateTime x) => x.DayOfWeek == DayOfWeek.Sunday;

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant", Justification = "Dummy justification.")]
		public bool Bar(DateTime x) => x.Day != 2;
	}
}
