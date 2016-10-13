public class Bob
{
	public void Method(int value)
	{
		int unvarableLocal; // can't translate this so don't suggest it
		int varableLocal = value;
		object wrongType = value; // can't translate this, so don't suggest it
		const int Value = 42; // can't use var on constants, so don't suggest it.
		unvarableLocal = value;
	}
}
