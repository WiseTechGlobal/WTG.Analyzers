using System;
using System.Runtime;

public class Bob
{
	public void Main()
	{
		double price = 20.00;
		string foo = "foo";

		string useful = $"This is not a useless interpolated string because it tells you a price: {price}";
		string useless = "This is useless and there is zero need to be";
		FormattableString s = $"This is not useless but, it looks useless at first glance";
		Method1("This is useless and there is zero need to be");
		Method2($"This is not useless but, it looks useless at first glance");
		Method1(foo);
		Method1(price.ToString());
		Method1("");
		Method1("this is a literal...");
		Method1(foo);

		Method1(useless);
		Method1(useful);
		Method2(s);
	}

	public string Method1(string s) => s;
	public IFormattable Method2(FormattableString s) => s;
}
