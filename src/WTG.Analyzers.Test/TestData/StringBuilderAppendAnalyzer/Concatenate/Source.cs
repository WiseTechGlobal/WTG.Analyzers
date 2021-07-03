using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo + bar);
			builder.AppendLine(bar + foo);
			builder.AppendLine(foo + bar);
		}
	}
}
