using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.TestFramework
{
	internal static class DiagnosticConversion
	{
		public static DiagnosticResult Convert(Diagnostic diagnostic)
		{
			var builder = ImmutableArray.CreateBuilder<DiagnosticResultLocation>(1 + diagnostic.AdditionalLocations.Count);

			if (diagnostic.Location.Kind != LocationKind.None)
			{
				builder.Add(Convert(diagnostic.Location));
			}

			for (var i = 0; i < diagnostic.AdditionalLocations.Count; i++)
			{
				builder.Add(Convert(diagnostic.AdditionalLocations[i]));
			}

			return new DiagnosticResult(
				diagnostic.Id,
				diagnostic.Severity,
				diagnostic.GetMessage(),
				builder.ToImmutable());
		}

		public static DiagnosticResultLocation Convert(Location location)
		{
			var loc = location.GetLineSpan();

			return new DiagnosticResultLocation(
				loc.Path,
				loc.StartLinePosition.Line + 1,
				loc.StartLinePosition.Character + 1,
				loc.EndLinePosition.Line + 1,
				loc.EndLinePosition.Character + 1);
		}
	}
}
