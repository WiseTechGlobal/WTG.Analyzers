using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo).Append(bar);
			builder.Append(bar).AppendLine(foo);
			builder.Append(foo).Append(bar).AppendLine();
		}
	}
}
