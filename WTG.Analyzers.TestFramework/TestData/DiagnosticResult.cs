using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.TestFramework
{
	public sealed class DiagnosticResult
	{
		public DiagnosticResult(string id, DiagnosticSeverity severity, string message, ImmutableArray<DiagnosticResultLocation> locations)
		{
			if (id == null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			Id = id;
			Severity = severity;
			Message = message;
			Locations = locations;
		}

		public string Id { get; }
		public DiagnosticSeverity Severity { get; }
		public string Message { get; }
		public ImmutableArray<DiagnosticResultLocation> Locations { get; }
	}
}
