using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Test.Helpers
{
	public struct DiagnosticResult
	{
		public DiagnosticResult(string id, DiagnosticSeverity severity, string message, DiagnosticResultLocation[] locations)
		{
			Id = id;
			Severity = severity;
			Message = message;
			Locations = ImmutableArray.Create(locations);
		}

		public string Id { get; }
		public DiagnosticSeverity Severity { get; }
		public string Message { get; }
		public ImmutableArray<DiagnosticResultLocation> Locations { get; }
	}
}
