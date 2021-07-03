using System.Diagnostics.Contracts;

public class Bob
{
	public Bob()
	{
		value = new object();
	}

	[ContractInvariantMethod]
	void ObjectInvariant()
	{
		Contract.Invariant(value != null);
	}

	object value;
}
