using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	static class CommonDiagnosticProperties
	{
		public const string CanAutoFixProperty = "CanAutoFix";

		public static bool CanAutoFix(Diagnostic diagnostic)
		{
			if (!diagnostic.Properties.TryGetValue(CanAutoFixProperty, out var valueStr) ||
				!bool.TryParse(valueStr, out var value))
			{
				return true;
			}

			return value;
		}

		public static ImmutableDictionary<string, string?> NoAutoFixProperties = ImmutableDictionary<string, string?>.Empty.Add(CanAutoFixProperty, bool.FalseString);
	}
}
