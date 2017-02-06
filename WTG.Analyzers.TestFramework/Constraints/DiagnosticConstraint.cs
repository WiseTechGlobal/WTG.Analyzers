using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework.Constraints;

namespace WTG.Analyzers.TestFramework
{
	public sealed class DiagnosticConstraint : Constraint
	{
		public DiagnosticConstraint(IEnumerable<DiagnosticResult> expected)
			: base(expected)
		{
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (actual == null)
			{
				return new ConstraintResult(this, actual, Arguments[0] == null);
			}

			var tmp = actual as IEnumerable<Diagnostic>;

			if (tmp == null)
			{
				return new ConstraintResult(this, actual);
			}

			return ApplyTo(
				tmp.Select(DiagnosticConversion.Convert),
				(IEnumerable<DiagnosticResult>)Arguments[0]);
		}

		ConstraintResult ApplyTo(IEnumerable<DiagnosticResult> actual, IEnumerable<DiagnosticResult> expected)
		{
			var result = new List<Tuple<DiagnosticResult, DiagnosticResult>>();

			using (var a = actual.OrderBy(x => x, DiagnosticResultComparer.Instance).GetEnumerator())
			using (var e = expected.OrderBy(x => x, DiagnosticResultComparer.Instance).GetEnumerator())
			{
				var hasActual = a.MoveNext();
				var hasExpected = e.MoveNext();

				while (hasActual && hasExpected)
				{
					var diff = DiagnosticResultComparer.Compare(a.Current, e.Current);

					if (diff < 0)
					{
						result.Add(Tuple.Create<DiagnosticResult, DiagnosticResult>(a.Current, null));
						hasActual = a.MoveNext();
					}
					else if (diff > 0)
					{
						result.Add(Tuple.Create<DiagnosticResult, DiagnosticResult>(null, e.Current));
						hasExpected = e.MoveNext();
					}
					else
					{
						hasActual = a.MoveNext();
						hasExpected = e.MoveNext();
					}
				}

				if (hasActual)
				{
					do
					{
						result.Add(Tuple.Create<DiagnosticResult, DiagnosticResult>(a.Current, null));
					}
					while (a.MoveNext());
				}
				else if (hasExpected)
				{
					do
					{
						result.Add(Tuple.Create<DiagnosticResult, DiagnosticResult>(null, e.Current));
					}
					while (e.MoveNext());
				}
			}

			if (result.Count == 0)
			{
				return new ConstraintResult(this, actual, true);
			}

			return new DiagnosticConstraintResult(this, result);
		}

		sealed class DiagnosticConstraintResult : ConstraintResult
		{
			public DiagnosticConstraintResult(Constraint constraint, List<Tuple<DiagnosticResult, DiagnosticResult>> differences)
				: base(constraint, differences)
			{
				this.differences = differences;
			}

			public override void WriteMessageTo(MessageWriter writer)
			{
				foreach (var tuple in differences)
				{
					if (tuple.Item1 == null)
					{
						writer.Write('-');
						Write(writer, tuple.Item2);
					}
					else
					{
						writer.Write('+');
						Write(writer, tuple.Item1);
					}

					writer.WriteLine();
				}
			}

			void Write(MessageWriter writer, DiagnosticResult diagnostic)
			{
				if (diagnostic.Locations.Length > 0)
				{
					writer.Write(diagnostic.Locations[0].ToString());
				}

				writer.Write(": ");
				writer.Write(diagnostic.Severity);
				writer.Write(": ");
				writer.Write(diagnostic.Id);
				writer.Write(": ");
				writer.Write(diagnostic.Message);

				for (var i = 1; i < diagnostic.Locations.Length; i++)
				{
					writer.Write(", ");
					writer.Write(diagnostic.Locations[i].ToString());
				}
			}

			readonly List<Tuple<DiagnosticResult, DiagnosticResult>> differences;
		}
	}
}
