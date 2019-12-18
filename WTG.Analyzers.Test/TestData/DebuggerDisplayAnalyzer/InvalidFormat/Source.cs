using System.Diagnostics;

[DebuggerDisplay("Magic = {")]
class Unclosed
{
	public int Property => 32;
}

[DebuggerDisplay("Magic = }")]
class Unbalanced
{
	public int Property => 32;
}

[DebuggerDisplay("Magic = {Property,}")]
class Partial
{
	public int Property => 32;
}
