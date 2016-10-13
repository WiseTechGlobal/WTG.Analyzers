using WTG.Analyzers.Test.Helpers;

namespace WTG.Analyzers.Test.Constraints
{
	internal static class IsDiagnostic
	{
		public static DiagnosticConstraint EqualTo(DiagnosticResult expected) => new DiagnosticConstraint(expected);
	}
}
