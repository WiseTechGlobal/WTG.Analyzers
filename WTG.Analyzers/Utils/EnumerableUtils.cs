using System.Collections;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	internal static class EnumerableUtils
	{
		// get the element type from an enumerable using the same logic as foreach
		// (foreach does not require the enumerable to implement IEnumerable<>)
		public static ITypeSymbol GetElementType(ITypeSymbol enumerableType)
		{
			return GetExplicitElementType(enumerableType) ?? GetImplicitElementType(enumerableType);
		}

		static ITypeSymbol GetExplicitElementType(ITypeSymbol enumerableType)
		{
			foreach (var method in enumerableType.GetMembers(nameof(IEnumerable.GetEnumerator)).OfType<IMethodSymbol>())
			{
				if (!method.IsGenericMethod && method.Parameters.Length == 0 && method.TypeArguments.Length == 0)
				{
					var retType = method.ReturnType;

					return GetElementTypeFromEnumeratorType(retType);
				}
			}

			return null;
		}

		static ITypeSymbol GetImplicitElementType(ITypeSymbol enumerableType)
		{
			var candidateTypes = Enumerable.ToArray(
				from iface in enumerableType.AllInterfaces
				let elementType = GetElementTypeFromEnumerableInterface(iface)
				where elementType != null
				select elementType);

			ITypeSymbol suggestedType = null;

			foreach (var type in candidateTypes)
			{
				if (suggestedType == null || suggestedType.SpecialType == SpecialType.System_Object)
				{
					suggestedType = type;
				}
				else if (type.SpecialType != SpecialType.System_Object)
				{
					// close enough to ambigious.
					return null;
				}
			}

			return suggestedType;
		}

		static ITypeSymbol GetElementTypeFromEnumeratorType(ITypeSymbol retType)
		{
			while (true)
			{
				ITypeSymbol itemType = null;

				foreach (var member in retType.GetMembers())
				{
					if (member.IsImplicitlyDeclared) continue;

					var property = member as IPropertySymbol;

					if (property != null && property.Name == nameof(IEnumerator.Current))
					{
						if (itemType != null)
						{
							return null;
						}

						itemType = property.Type;
					}
				}

				if (itemType != null)
				{
					return itemType;
				}
				else if (retType == null)
				{
					return null;
				}

				retType = retType.BaseType;
			}
		}

		static ITypeSymbol GetElementTypeFromEnumerableInterface(ITypeSymbol type)
		{
			var namedType = type as INamedTypeSymbol;

			if (namedType == null)
			{
				return null;
			}

			switch (namedType.SpecialType)
			{
				case SpecialType.System_Collections_IEnumerable:
				case SpecialType.System_Collections_Generic_IEnumerator_T:
					return GetExplicitElementType(type);
			}

			return null;
		}
	}
}
