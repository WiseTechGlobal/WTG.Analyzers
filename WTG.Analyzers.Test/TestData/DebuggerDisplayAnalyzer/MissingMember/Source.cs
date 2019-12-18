using System.Diagnostics;

[DebuggerDisplay("Magic = {Property}")]
class MissingProperty
{
}

[DebuggerDisplay("Magic = {Method(Property)}")]
class MissingMethod
{
	public int Property => 32;
}

[DebuggerDisplay("Magic = {Method(Property)}")]
class MissingArgument
{
	public string Method(int value) => null;
}
