using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Match("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Match("Foo", "Bar", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex.Match("Foo", "Bar", RegexOptions.Compiled);
			Regex.Match("Foo", "Bar");
		}
	}
}
