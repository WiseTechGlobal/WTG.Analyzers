using System.Diagnostics.Contracts;

namespace Foo
{
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
}
