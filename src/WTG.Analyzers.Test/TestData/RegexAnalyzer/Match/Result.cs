using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Match("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Match("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Match("Foo", "Bar");
			Regex.Match("Foo", "Bar");
		}
	}
}
