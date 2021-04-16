using System.Text;

namespace Magic
{
	class Bob
	{
		void Method(string foo, int bar)
		{
			var builder = new StringBuilder();
			builder.Append(foo);
			builder.Append(bar + 5); // adding int's, not concatenating strings.
			builder.Append(Decoy(foo, bar));
			builder.AppendFormat("Prefix {0} Suffix", foo);
			builder.AppendLine();
			Decoy(foo + bar,bar);
		}

		static string Decoy(string value, int bar) => value;
	}
}
