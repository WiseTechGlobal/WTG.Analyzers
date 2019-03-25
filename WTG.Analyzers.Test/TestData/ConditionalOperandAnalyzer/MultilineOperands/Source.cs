public class Bob
{
	public object Method(bool flag1) =>
		flag1
			? new
			{
				Name = "Foo"
			}
			: new
			{
				Name = "Bar"
			};
}
