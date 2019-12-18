using System.Diagnostics;

[DebuggerDisplay("Value = {MethodValue()}")]
class InstanceMethod
{
	public int MethodValue() => 42;
}

[DebuggerDisplay("Value = {MethodValue(PropertyValue)}")]
class InstanceMethodWithArgument
{
	public int PropertyValue => 32;
	public int MethodValue(int x) => x;
}

[DebuggerDisplay("Value = {MethodValue(PropertyValue)}")]
class LocalStaticMethodWithArgument
{
	public int PropertyValue => 32;
	public static int MethodValue(int x) => x;
}

[DebuggerDisplay("Count = System.Linq.Enumerable.Count(values)")]
class ExternalStaticMethod
{
	int[] values = new[] { 1, 2, 3 };
}
