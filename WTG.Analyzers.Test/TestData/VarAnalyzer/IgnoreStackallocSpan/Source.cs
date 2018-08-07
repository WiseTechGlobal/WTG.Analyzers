using System;

public static class Bob
{
	public static void Method()
	{
		Span<byte> local = stackalloc byte[10];
	}
}

namespace System
{
	public readonly ref struct Span<T>
	{
		public unsafe Span(void * ptr, int count)
		{
		}
	}
}
