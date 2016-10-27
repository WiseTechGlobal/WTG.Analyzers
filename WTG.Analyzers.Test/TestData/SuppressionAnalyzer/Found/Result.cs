[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestClass")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Method()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Property")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#field")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestClass+NestedClass")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#.cctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#.ctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#GenericMethod`2(Func`2<!!0,!!1>,!!0)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Test.Namespace.TestInterface`1<System.Int32>.InterfaceMember")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestInterface`1")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#ByRef(System.String&)")]

namespace Test.Namespace
{
	class TestClass : TestInterface<int>
	{
		static TestClass() { }
		public TestClass() { }
		int Method() => field;
		Func<T> GenericMethod<V, T>(Func<V, T> func, V value) => () => func(value);
		int Property => field;
		int field;

		bool ByRef(out string value)
		{
			value = null;
			return false;
		}

		int TestInterface<int>.InterfaceMember => field;

		class NestedClass { }
	}

	interface TestInterface<V>
	{
		V InterfaceMember { get; }
	}
}
