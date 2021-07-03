using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace1")]

[assembly:
	SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace2")]

namespace Test.Namespace1
{
}

namespace Test.Namespace2
{
}
