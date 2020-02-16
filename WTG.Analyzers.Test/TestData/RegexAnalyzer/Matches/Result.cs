using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Matches("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Matches("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Matches("Foo", "Bar");
			Regex.Matches("Foo", "Bar");
		}
	}
}
