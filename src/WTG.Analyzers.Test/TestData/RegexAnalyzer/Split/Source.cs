using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Split("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Split("Foo", "Bar", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex.Split("Foo", "Bar", RegexOptions.Compiled);
			Regex.Split("Foo", "Bar");
		}
	}
}
