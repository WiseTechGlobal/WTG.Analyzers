using System;

public class Bob
{
	public void Main()
	{
		double price = 20.00;
		string foo = "foo";

		string useful = $"This is not a useless interpolated string because it tells you a price: {price}";
		string useless = $"This is useless and there is zero need to be";
		FormattableString s = $"This is not useless but, it looks useless at first glance";
		Method1($"This is useless and there is zero need to be");
		Method2($"This is not useless but, it looks useless at first glance");
		Method1($"{foo}");
		Method1($"{price}");
		Method1($"");
		Method1($"{"this is a literal..."}");
		Method1($"{"literal with alignment and format for no good reason",123:ABC}");
		Method1(@$"{foo}");

		Method1(useless);
		Method1(useful);
		Method2(s);

		Method1($"{price:d}");
		Method1($"{price,8}");
		Method1($"{price,8:d}");
		Method1($"{null}");
	}

	public string Method1(string s) => s;
	public IFormattable Method2(FormattableString s) => s;
}
