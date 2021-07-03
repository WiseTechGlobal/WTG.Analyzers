public class Bob
{
	public string Method(bool test) => test ? "A" : "B";

	public string Property => flag ? "A" : "B";
	public string this[bool test] => test ? "A" : "B";

	bool flag;
}
