namespace WTG.Analyzers.Utils
{
	public static class WellKnownTypeNames
	{
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable SA1310 // Field names should not contain underscore
		public const string Enumerable = "System.Linq.Enumerable";
		public const string IQueryable_T = "System.Linq.IQueryable`1";
		public const string Span = "System.Span`1";
		public const string Task = "System.Threading.Tasks.Task";
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore CA1707 // Identifiers should not contain underscores
	}
}
