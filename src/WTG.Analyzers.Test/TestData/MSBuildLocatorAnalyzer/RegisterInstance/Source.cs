using Microsoft.Build.Locator;
using MSBuildLocatorByAnyOtherName = Microsoft.Build.Locator.MSBuildLocator;

namespace Microsoft.Build.Locator
{
	public class VisualStudioInstance
	{
	}

	public class MSBuildLocator
	{
		public static void RegisterInstance(VisualStudioInstance instance) => throw null;
	}
}

class Foo
{
	public void Method(VisualStudioInstance instance)
	{
		MSBuildLocator.RegisterInstance(instance);
		MSBuildLocatorByAnyOtherName.RegisterInstance(instance);
	}
}
