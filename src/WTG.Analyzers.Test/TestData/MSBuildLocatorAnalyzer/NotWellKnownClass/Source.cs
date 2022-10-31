public class VisualStudioInstance
{
}

public class MSBuildLocator
{
	public static VisualStudioInstance RegisterDefaults() => throw null;
}

class Foo
{
	public static void RegisterDefaults() => throw null;

	public void Method()
	{
		Foo.RegisterDefaults();
		MSBuildLocator.RegisterDefaults();
	}
}
