using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

public static class Bob
{
	[Pure, SuppressMessage("category", "checkid")]
	[ContractVerification(false)]
	public static string Method(string arg1, string arg2)
	{
		Contract.Requires(arg1 != null);
		Contract.Requires(!string.IsNullOrEmpty(arg2));
		Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
		return arg1 + arg2;
	}
}
