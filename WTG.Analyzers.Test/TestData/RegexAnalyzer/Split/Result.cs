using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			Regex.Split("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Split("Foo", "Bar", RegexOptions.IgnoreCase);
			Regex.Split("Foo", "Bar");
			Regex.Split("Foo", "Bar");
		}
	}
}
