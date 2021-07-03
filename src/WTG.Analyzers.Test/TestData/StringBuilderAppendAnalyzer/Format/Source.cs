using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(string.Format("Foo({0}) Bar({1})", foo, bar));
			builder.AppendLine(string.Format("Foo({0}) Bar({1})", foo, bar));
		}
	}
}
