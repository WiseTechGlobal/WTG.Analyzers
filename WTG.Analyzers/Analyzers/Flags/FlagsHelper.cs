using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class FlagsHelper
	{
		public static bool IsNone(EnumMemberDeclarationSyntax member)
		{
			return member.Identifier.Text == "None";
		}

		public static long AvailableBitMask(SemanticModel model, EnumDeclarationSyntax decl)
		{
			var result = 0L;

			foreach (var member in decl.Members)
			{
				if (member.EqualsValue != null)
				{
					var value = model.GetConstantValue(member.EqualsValue.Value);

					if (value.HasValue)
					{
						result |= AsLong(value.Value);
					}
				}
			}

			return ~result;
		}

		public static int TakeNextAvailableIndex(ref long available)
		{
			var tmp = available;

			if (tmp == 0)
			{
				return 64;
			}

			available = tmp & (tmp - 1);
			tmp ^= available;

			var count = 0;

			while (tmp != 0)
			{
				tmp >>= 1;
				count++;
			}

			return count - 1;
		}

		static long AsLong(object value)
		{
			if (value == null)
			{
				return 0;
			}

			if (value is int)
			{
				return (int)value;
			}

			return (long)value;
		}
	}
}
