using System.Diagnostics.Contracts;

namespace Foo
{
	[ContractClass(typeof(ContractClassForIBob))]
	public interface IBob
	{
		void Method();
	}

	abstract class ContractClassForIBob : IBob
	{
		public void Method()
		{
		}
	}
}
