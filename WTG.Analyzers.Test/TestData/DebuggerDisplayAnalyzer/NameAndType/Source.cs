using System.Diagnostics;

[DebuggerDisplay("Value = {Property}", Name = "Name = {Property}", Type = "Type = {Property}")]
class Success
{
	public int Property = 0;
}

[DebuggerDisplay("Value = {Property}", Name = "Name = {Property}", Type = "Type = {Property}")]
class Failure
{
}
