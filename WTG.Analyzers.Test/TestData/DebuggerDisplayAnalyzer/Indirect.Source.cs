using System.Diagnostics;

[DebuggerDisplay("Value = {Related.ID}")]
class IndirectProperty
{
	public int ID = 5;
	public IndirectField Related => new IndirectField();
}

[DebuggerDisplay("Value = {Related.ID}")]
class IndirectField
{
	public int ID { get; } = 5;
	public IndirectProperty Related => new IndirectProperty();
}
