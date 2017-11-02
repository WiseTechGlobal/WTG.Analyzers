using System.Diagnostics.CodeAnalysis;

[assembly:
	SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Missing1"),
	SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Missing2")
]

[assembly: SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Missing"), SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace1")]

[assembly:
	SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace2"),
	SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Missing")
]

namespace Test.Namespace1
{
}

namespace Test.Namespace2
{
}
