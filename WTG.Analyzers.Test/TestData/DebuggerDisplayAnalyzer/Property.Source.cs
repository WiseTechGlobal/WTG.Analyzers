using System.Diagnostics;

[DebuggerDisplay("Value = {PropertyValue}")]
class Property
{
	public int PropertyValue => 32;
}

[DebuggerDisplay("Value = {PropertyValue}")]
class PrivateProperty
{
	int PropertyValue => 32;
}
