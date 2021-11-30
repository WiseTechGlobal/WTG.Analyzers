using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(char c, int repeat)
		{
			var builder = new StringBuilder();
			builder.Append(c, repeat);
		}
	}
}
