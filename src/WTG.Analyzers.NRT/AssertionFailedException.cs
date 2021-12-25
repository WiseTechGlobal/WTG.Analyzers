using System;

#if DEBUG
namespace WTG.Analyzers
{
#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA1064 // Exceptions should be public
	sealed class AssertionFailedException : Exception
	{
		public AssertionFailedException(string message) : base(message)
		{
		}
	}
#pragma warning restore CA1064 // Exceptions should be public
#pragma warning restore CA1032 // Implement standard exception constructors
}
#endif
