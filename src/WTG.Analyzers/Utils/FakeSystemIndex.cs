using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
#pragma warning disable CA1822
	struct Index
	{
		public Index (int value, bool fromEnd = false)
		{
		}

		public int GetOffset(int length) => 0;
	}
#pragma warning restore CA1822
}
