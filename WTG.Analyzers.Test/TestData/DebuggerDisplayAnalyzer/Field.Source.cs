using System.Diagnostics;

[DebuggerDisplay("Value = {FieldValue}")]
class Field
{
	public int FieldValue = 32;
}

[DebuggerDisplay("Value = {FieldValue}")]
class PrivateField
{
	int FieldValue = 32;
}
