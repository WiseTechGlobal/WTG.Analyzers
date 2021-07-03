using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method(RegexOptions options)
		{
			Regex.IsMatch("Foo", "Bar", RegexOptions.Compiled & options);
			Regex.IsMatch("Foo", "Bar", options & RegexOptions.Compiled);
			Regex.IsMatch("Foo", "Bar", options & (RegexOptions.Compiled | RegexOptions.IgnoreCase));
			Regex.IsMatch("Foo", "Bar", options & ~RegexOptions.Compiled);
		}
	}
}
