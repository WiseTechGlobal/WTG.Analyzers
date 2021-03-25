using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo.Substring(3, bar));
			builder.AppendLine(foo.Substring(3, bar));

			builder.Append(foo.Substring(3));
			builder.AppendLine(foo.Substring(3));
		}
	}
}
