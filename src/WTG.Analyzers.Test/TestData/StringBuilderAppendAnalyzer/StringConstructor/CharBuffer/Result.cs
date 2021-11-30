using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(char[] buffer, int start, int length)
		{
			var builder = new StringBuilder();
			builder.Append(buffer);
			builder.Append(buffer, start, length);
		}
	}
}
