using System;
using System.Collections;
using System.Collections.Generic;

namespace WTG.Analyzers.TestFramework
{
	sealed class DiagnosticResultComparer : IComparer<DiagnosticResult>, IComparer
	{
		public static DiagnosticResultComparer Instance { get; } = new DiagnosticResultComparer();

		DiagnosticResultComparer()
		{
		}

		public static int Compare(DiagnosticResult? x, DiagnosticResult? y)
		{
			switch (x, y)
			{
				case (null, null):
					return 0;
				case (null, _):
					return -1;
				case (_, null):
					return 1;
			}

			var firstLocX = x.Locations.Length == 0 ? null : x.Locations[0];
			var firstLocY = y.Locations.Length == 0 ? null : y.Locations[0];
			var diff = Compare(firstLocX, firstLocY);

			if (diff != 0)
			{
				return diff;
			}

			diff = string.CompareOrdinal(x.Id, y.Id);

			if (diff != 0)
			{
				return diff;
			}

			diff = x.Severity.CompareTo(y.Severity);

			if (diff != 0)
			{
				return diff;
			}

			diff = string.CompareOrdinal(x.Message, y.Message);

			if (diff != 0)
			{
				return diff;
			}

			var i = 1;

			for (; i < x.Locations.Length && i < y.Locations.Length; i++)
			{
				diff = Compare(x.Locations[i], y.Locations[i]);

				if (diff != 0)
				{
					return diff;
				}
			}

			if (i < x.Locations.Length)
			{
				return 1;
			}
			else if (i < y.Locations.Length)
			{
				return -1;
			}

			return 0;
		}

		static int Compare(DiagnosticResultLocation? x, DiagnosticResultLocation? y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			else if (y == null)
			{
				return 1;
			}

			var diff = string.Compare(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);

			if (diff != 0)
			{
				return diff;
			}

			diff = x.StartLine.CompareTo(y.StartLine);

			if (diff != 0)
			{
				return diff;
			}

			diff = x.StartColumn.CompareTo(y.StartColumn);

			if (diff != 0)
			{
				return diff;
			}

			diff = x.EndLine.CompareTo(y.EndLine);

			if (diff != 0)
			{
				return diff;
			}

			diff = x.EndColumn.CompareTo(y.EndColumn);
			return diff;
		}

		int IComparer.Compare(object? x, object? y) => Compare((DiagnosticResult?)x, (DiagnosticResult?)y);
		int IComparer<DiagnosticResult>.Compare(DiagnosticResult? x, DiagnosticResult? y) => Compare(x, y);
	}
}
