public class Bob
{
	public void Method(int value)
	{
		int unvarableLocal; // can't translate this so don't suggest it
		var varableLocal = value;
		object wrongType = value; // can't translate this, so don't suggest it
		unvarableLocal = value;
	}
}
