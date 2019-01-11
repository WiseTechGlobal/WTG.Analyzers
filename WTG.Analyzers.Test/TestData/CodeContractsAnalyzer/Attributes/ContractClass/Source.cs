using System.Diagnostics.Contracts;

[ContractClass(typeof(ContractClassForIBob))]
public interface IBob
{
	void Method();
}

[ContractClassFor(typeof(IBob))]
abstract class ContractClassForIBob : IBob
{
	public void Method()
	{
	}
}
