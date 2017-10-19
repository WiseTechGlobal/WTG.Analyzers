using System;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "namespace", Target = "Test.Namespace")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestClass")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Method()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Property")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#field")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestClass+NestedClass")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#.cctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#.ctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#GenericMethod`2(System.Func`2<!!0,!!1>,!!0)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#Test.Namespace.TestInterface`1<System.Int32>.InterfaceMember")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "type", Target = "Test.Namespace.TestInterface`1")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#ByRef(System.String&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#op_Implicit(Test.Namespace.TestClass):System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#op_Explicit(Test.Namespace.TestClass):System.Int32")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#op_Implicit(System.String):Test.Namespace.TestClass")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Pedantic", "CA9999:Magic", Scope = "member", Target = "Test.Namespace.TestClass.#op_Explicit(System.Int32):Test.Namespace.TestClass")]

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

		public static implicit operator string(TestClass bob) => bob.ToString();
		public static explicit operator int(TestClass bob) => int.Parse(bob.ToString());
		public static implicit operator TestClass(string bob) => null;
		public static explicit operator TestClass(int bob) => null;

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
