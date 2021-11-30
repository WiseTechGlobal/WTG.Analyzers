using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(char[] buffer, int start, int length)
		{
			var builder = new StringBuilder();
			builder.Append(new string(buffer));
			builder.Append(new string(buffer, start, length));
		}
	}
}
