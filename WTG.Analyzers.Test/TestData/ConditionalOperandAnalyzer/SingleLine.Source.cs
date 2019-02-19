public class Bob
{
	public string Method1(bool flag1) => flag1 ? "A" : "B";

	public string Method2(bool flag1) =>
		flag1 ? "A" : "B";

	public string Method3(bool flag1) =>
		flag1
			? "A"
			: "B";
}
