using System.Diagnostics.Contracts;

namespace Foo
{
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