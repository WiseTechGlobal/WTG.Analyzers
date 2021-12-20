using System;

namespace WTG.Analyzers.TestFramework
{
	[Flags]
	public enum SampleDataSetOptions
	{
		None = 0,
		OmitAssemblyReferences = 1 << 0,
	}
}
