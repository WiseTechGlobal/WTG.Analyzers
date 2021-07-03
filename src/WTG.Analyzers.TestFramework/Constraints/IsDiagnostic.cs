using System.Collections.Generic;
using System.Linq;

namespace WTG.Analyzers.TestFramework
{
	public static class IsDiagnostic
	{
		public static DiagnosticConstraint Empty => EqualTo(Enumerable.Empty<DiagnosticResult>());
		public static DiagnosticConstraint EqualTo(IEnumerable<DiagnosticResult> expected) => new DiagnosticConstraint(expected);
	}
}
