using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append($"Foo({foo}) Bar({bar})");
			builder.AppendLine($"Foo({foo}) Bar({bar})");
		}
	}
}
