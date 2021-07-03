public class Bob
{
	public string Method(bool test)
	{
		return test ? "A" : "B";
	}

	public string Property
	{
		get { return flag ? "A" : "B"; }
	}

	public string this[bool test]
	{
		get { return test ? "A" : "B"; }
	}

	bool flag;
}
