using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method(RegexOptions options)
		{
			Regex.IsMatch("Foo", "Bar");
			Regex.IsMatch("Foo", "Bar");
			Regex.IsMatch("Foo", "Bar", options & RegexOptions.IgnoreCase);
			Regex.IsMatch("Foo", "Bar", options & ~RegexOptions.Compiled);
		}
	}
}
