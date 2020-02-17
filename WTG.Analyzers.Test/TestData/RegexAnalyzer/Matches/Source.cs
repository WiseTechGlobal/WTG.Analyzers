using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Matches("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Matches("Foo", "Bar", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex.Matches("Foo", "Bar", RegexOptions.Compiled);
			Regex.Matches("Foo", "Bar");
		}
	}
}
