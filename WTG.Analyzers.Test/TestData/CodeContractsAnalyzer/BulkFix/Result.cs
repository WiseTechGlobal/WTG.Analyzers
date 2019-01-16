using System.Diagnostics.CodeAnalysis;

public static class Bob
{
	[SuppressMessage("category", "checkid")]
	public static string Method(string arg1, string arg2)
	{
		if (arg1 == null)
		{
			throw new System.ArgumentNullException(nameof(arg1));
		}

		if (string.IsNullOrEmpty(arg2))
		{
			throw new System.ArgumentException("Value cannot be null or empty.", nameof(arg2));
		}

		return arg1 + arg2;
	}
}
