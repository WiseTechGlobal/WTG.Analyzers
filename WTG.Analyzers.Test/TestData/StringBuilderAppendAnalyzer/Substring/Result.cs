using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo, 3, bar);
			builder.Append(foo, 3, bar).AppendLine();

			builder.Append(foo, 3, foo.Length - 3);
			builder.Append(foo, 3, foo.Length - 3).AppendLine();
		}
	}
}
