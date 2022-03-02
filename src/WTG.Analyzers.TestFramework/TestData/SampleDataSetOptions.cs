using System;

namespace WTG.Analyzers.TestFramework
{
	[Flags]
	public enum SampleDataSetOptions
	{
		None = 0,
		OmitAssemblyReferences = 1 << 0,
		AllowCodeFixes = 1 << 1,
		DisableNRT = 1 << 2,
	}
}
