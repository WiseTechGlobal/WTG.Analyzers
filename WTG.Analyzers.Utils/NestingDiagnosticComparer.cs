using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace WTG.Analyzers.Utils
{
	/// <summary>
	/// Compares diagnostics with a primary ordering of ascending start and a secondary
	/// ordering of decending end. This places inner diagnostics before any encapsulating diagnostics.
	/// </summary>
	public sealed class NestingComparer : IComparer<TextSpan>
	{
		public static NestingComparer Instance { get; } = new NestingComparer();

		NestingComparer()
		{
		}

		public static int Compare(TextSpan x, TextSpan y)
		{
			var diff = x.Start.CompareTo(y.Start);

			if (diff == 0)
			{
				diff = y.End.CompareTo(x.End);
			}

			return diff;
		}

		int IComparer<TextSpan>.Compare(TextSpan x, TextSpan y) => Compare(x, y);
	}
}
