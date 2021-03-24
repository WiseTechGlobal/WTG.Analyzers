using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Foo({0}) Bar({1})", foo, bar);
			builder.AppendFormat("Foo({0}) Bar({1})", foo, bar).AppendLine();
		}
	}
}
