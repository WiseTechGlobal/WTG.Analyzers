using System.Diagnostics;

[DebuggerDisplay("Magic = {Property1[Property2]}")]
class MissingProperty
{
	public int Property2 => 0;
}

[DebuggerDisplay("Magic = {Property1[Property2]}")]
class MissingArgument
{
	public int[] Property1 => new[] { 32 };
}

[DebuggerDisplay("Magic = {Property1[Property2]}")]
class NotIndexable
{
	public int Property1 => 32;
	public int Property2 => 0;
}

[DebuggerDisplay("Magic = {Property1[Property2]}")]
class Success
{
	public int[] Property1 => new[] { 32 };
	public int Property2 => 0;
}
