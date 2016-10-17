using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework.Constraints;
using WTG.Analyzers.Test.Helpers;

namespace WTG.Analyzers.Test.Constraints
{
	internal sealed class DiagnosticConstraint : Constraint
	{
		public DiagnosticConstraint(DiagnosticResult result)
			: base(result)
		{
			Description = FormattableString.Invariant($"{result.Severity}: {result.Id}: {result.Message}{Environment.NewLine}    [ {string.Join(" | ", result.Locations)} ]");
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (actual == null)
			{
				return new ConstraintResult(this, actual, Arguments[0] == null);
			}

			var tmp = actual as Diagnostic;

			if (tmp == null)
			{
				return new ConstraintResult(this, actual);
			}

			return ApplyTo(tmp, (DiagnosticResult)Arguments[0]);
		}

		ConstraintResult ApplyTo(Diagnostic actual, DiagnosticResult expected)
		{
			var differences = GetDifferences(actual, expected).ToArray();

			if (differences.Length == 0)
			{
				return new ConstraintResult(this, actual, true);
			}

			return new DiagnosticConstraintResult(this, actual, differences);
		}

		static IEnumerable<Difference> GetDifferences(Diagnostic actual, DiagnosticResult expected)
		{
			if (actual.Id != expected.Id)
			{
				yield return new Difference(nameof(actual.Id), actual.Id);
			}

			if (actual.Severity != expected.Severity)
			{
				yield return new Difference(nameof(actual.Severity), actual.Severity);
			}

			var message = actual.GetMessage();

			if (message != expected.Message)
			{
				yield return new Difference(nameof(expected.Message), message);
			}

			if (expected.Locations.Length == 0)
			{
				if (actual.Location.Kind != LocationKind.None)
				{
					yield return new Difference(nameof(actual.Location), actual.Location);
				}

				for (var i = 0; i < actual.AdditionalLocations.Count; i++)
				{
					yield return new Difference(nameof(actual.AdditionalLocations) + "[" + i + "]", actual.AdditionalLocations[i]);
				}
			}
			else
			{
				if (!IsMatch(actual.Location, expected.Locations[0]))
				{
					yield return new Difference(nameof(actual.Location), actual.Location);
				}

				var commonLength = Math.Min(actual.AdditionalLocations.Count, expected.Locations.Length - 1);

				for (var i = 0; i < commonLength; i++)
				{
					if (!IsMatch(actual.AdditionalLocations[i], expected.Locations[i + 1]))
					{
						yield return new Difference(nameof(actual.AdditionalLocations) + "[" + i + "]", actual.AdditionalLocations[i]);
					}
				}

				while (commonLength < expected.Locations.Length - 1)
				{
					yield return new Difference(nameof(actual.AdditionalLocations) + "[" + commonLength + "]", null);
					commonLength++;
				}

				while (commonLength < actual.AdditionalLocations.Count)
				{
					yield return new Difference(nameof(actual.AdditionalLocations) + "[" + commonLength + "]", actual.AdditionalLocations[commonLength]);
					commonLength++;
				}
			}
		}

		static bool IsMatch(Location actual, DiagnosticResultLocation expected)
		{
			var actualSpan = actual.GetLineSpan();

			return actualSpan.Path == expected.Path
				&& IsMatch(actualSpan.StartLinePosition, expected.StartLine, expected.StartColumn)
				&& IsMatch(actualSpan.EndLinePosition, expected.EndLine, expected.EndColumn);
		}

		static bool IsMatch(LinePosition pos, int line, int character)
		{
			return pos.Line + 1 == line
				&& pos.Character + 1 == character;
		}

		static string Format(Location location)
		{
			if (location == null)
			{
				return null;
			}

			var actualSpan = location.GetLineSpan();
			var startPos = actualSpan.StartLinePosition;
			var endPos = actualSpan.EndLinePosition;

			return DiagnosticResultLocation.Format(
				actualSpan.Path,
				startPos.Line + 1,
				startPos.Character + 1,
				endPos.Line + 1,
				endPos.Character + 1);
		}

		sealed class DiagnosticConstraintResult : ConstraintResult
		{
			public DiagnosticConstraintResult(DiagnosticConstraint constraint, Diagnostic value, Difference[] differences)
				: base(constraint, value)
			{
				this.differences = differences;
			}

			public override void WriteMessageTo(MessageWriter writer)
			{
				writer.Write("  Expected: ");
				writer.WriteLine(Description);

				const int MaxDifferenceCount = 3;

				foreach (var difference in differences.Take(MaxDifferenceCount))
				{
					writer.Write("  ");
					writer.Write(difference.Label);
					writer.Write(": ");
					WriteValue(writer, difference);
				}

				if (differences.Length > MaxDifferenceCount)
				{
					writer.Write("  ...");
				}
			}

			static void WriteValue(MessageWriter writer, Difference difference)
			{
				var location = difference.Value as Location;

				if (location != null)
				{
					writer.WriteLine(Format(location));
				}
				else
				{
					writer.WriteValue(difference.Value);
					writer.WriteLine();
				}
			}

			readonly Difference[] differences;
		}

		sealed class Difference
		{
			public Difference(string label, object value)
			{
				Label = label;
				Value = value;
			}

			public string Label { get; }
			public object Value { get; }
		}
	}
}
