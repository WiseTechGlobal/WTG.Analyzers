using System;

namespace WTG.Analyzers.TestFramework
{
	public class DiagnosticResultLocation
	{
		public DiagnosticResultLocation(string path, int startLine, int startColumn, int endLine, int endColumn)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (startLine < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startLine), startLine, "line must be non-negative.");
			}

			if (startColumn < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startColumn), startColumn, "column must be non-negative.");
			}

			if (endLine < startLine)
			{
				throw new ArgumentOutOfRangeException(nameof(endLine), endLine, "end line cannot be before the start line.");
			}

			if (endColumn < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(endColumn), endColumn, "column must be non-negative.");
			}

			Path = path;
			StartLine = startLine;
			StartColumn = startColumn;
			EndLine = endLine;
			EndColumn = endColumn;
		}

		public string Path { get; }
		public int StartLine { get; }
		public int StartColumn { get; }
		public int EndLine { get; }
		public int EndColumn { get; }

		public override string ToString()
		{
			return Format(Path, StartLine, StartColumn, EndLine, EndColumn);
		}

		static string Format(string path, int startLine, int startColumn, int endLine, int endColumn)
		{
			if (startLine != endLine)
			{
				return FormattableString.Invariant($"{path}: ({startLine},{startColumn}) - ({endLine},{endColumn})");
			}
			else if (startColumn != endColumn)
			{
				return FormattableString.Invariant($"{path}: ({startLine},{startColumn}-{endColumn})");
			}
			else
			{
				return FormattableString.Invariant($"{path}: ({startLine},{startColumn})");
			}
		}
	}
}
