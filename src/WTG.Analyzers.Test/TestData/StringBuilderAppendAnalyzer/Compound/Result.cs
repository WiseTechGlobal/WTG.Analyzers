using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo).Append(foo, bar, 3).AppendFormat("{0}:{1}", foo, bar);
		}
	}
}
