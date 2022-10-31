using Microsoft.Build.Locator;
using System;
using MSBuildLocatorByAnyOtherName = Microsoft.Build.Locator.MSBuildLocator;

namespace Microsoft.Build.Locator
{
	public class VisualStudioInstance
	{
	}

	public class MSBuildLocator
	{
		public static VisualStudioInstance RegisterDefaults() => throw null;
	}
}

class Foo
{
	public void Method()
	{
		MSBuildLocator.RegisterDefaults();
		MSBuildLocatorByAnyOtherName.RegisterDefaults();
		Func<object> asADelgate = MSBuildLocator.RegisterDefaults;
	}
}
