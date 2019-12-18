using System.Diagnostics;

[DebuggerDisplay("Value = {PropertyValue,nq}")]
class Property
{
	public int PropertyValue => 32;
}
