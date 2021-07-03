public class Bob
{
	public string Method1(bool flag1, bool flag2) => flag1 ? flag2 ? "A" : "B" : "C";
	public string Method2(bool flag1, bool flag2) => flag1 ? "C" : (flag2 ? "A" : "B");
}
