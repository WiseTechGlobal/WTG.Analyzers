using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.IsMatch("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.IsMatch("Foo", "Bar", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex.IsMatch("Foo", "Bar", RegexOptions.Compiled);
			Regex.IsMatch("Foo", "Bar");
		}
	}
}
