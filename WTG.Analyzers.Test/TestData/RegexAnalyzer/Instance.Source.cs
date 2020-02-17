using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void Method()
		{
			regex.Match("Bar");
			regex.Matches("Bar");
			regex.Split("Bar");
			regex.IsMatch("Bar");
			regex.Replace("Bar", "Baz");
			regex.Replace("Bar", m => "Baz");
		}

		static readonly Regex regex = new Regex("Foo", RegexOptions.Compiled);
	}
}
