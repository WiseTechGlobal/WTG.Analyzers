using System;
using System.Text.RegularExpressions;

namespace Magic
{
	class A
	{
		public void MethodA()
		{
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.IgnoreCase);
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.IgnoreCase);
			Regex.Replace("Foo", "Bar", "Baz");
			Regex.Replace("Foo", "Bar", "Baz");
		}

		public void MethodB()
		{
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.IgnoreCase);
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.IgnoreCase);
			Regex.Replace("Foo", "Bar", m => "Baz");
			Regex.Replace("Foo", "Bar", m => "Baz");
		}

		public void MethodC()
		{
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.None, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", "Baz", RegexOptions.None, TimeSpan.FromSeconds(1));
		}

		public void MethodD()
		{
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.None, TimeSpan.FromSeconds(1));
			Regex.Replace("Foo", "Bar", m => "Baz", RegexOptions.None, TimeSpan.FromSeconds(1));
		}
	}
}
