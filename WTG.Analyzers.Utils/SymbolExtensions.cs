using System;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	public static class SymbolExtensions
	{
		public static bool IsValueTuple(this ITypeSymbol typeSymbol)
		{
			return typeSymbol.MetadataName.StartsWith("ValueTuple`", StringComparison.Ordinal)
				&& typeSymbol.ContainingType == null
				&& typeSymbol.ContainingNamespace.IsMatch("System");
		}

		public static bool IsMatch(this IMethodSymbol methodSymbol, string fullTypeName, string methodName)
		{
			return methodSymbol.MetadataName == methodName
				&& methodSymbol.ContainingType.IsMatch(fullTypeName);
		}

		public static bool IsMatch(this ITypeSymbol typeSymbol, string fullName)
		{
			ISymbol symbol = typeSymbol;
			var length = fullName.Length;

			while (true)
			{
				var index = fullName.LastIndexOf('+', length - 1);

				if (index < 0)
				{
					break;
				}

				if (!MatchesSubstring(symbol.MetadataName, fullName, index + 1, length))
				{
					return false;
				}

				length = index;
				symbol = symbol.ContainingType;

				if (symbol == null)
				{
					return false;
				}
			}

			if (symbol.ContainingType != null)
			{
				return false;
			}

			return IsMatchCore(symbol, fullName, length);
		}

		public static bool IsMatch(this INamespaceSymbol namespaceSymbol, string fullName)
		{
			return IsMatchCore(namespaceSymbol, fullName, fullName.Length);
		}

		public static bool IsMatchAnyArity(this ITypeSymbol typeSymbol, string fullName)
		{
			if (typeSymbol.IsTupleType)
			{
				typeSymbol = ((INamedTypeSymbol)typeSymbol).TupleUnderlyingType;
			}

			ISymbol symbol = typeSymbol;
			var length = fullName.Length;

			while (true)
			{
				var index = fullName.LastIndexOf('+', length - 1);

				if (index < 0)
				{
					break;
				}

				if (!MatchesSubstring(symbol.Name, fullName, index + 1, length))
				{
					return false;
				}

				length = index;
				symbol = symbol.ContainingType;

				if (symbol == null)
				{
					return false;
				}
			}

			if (symbol.ContainingType != null)
			{
				return false;
			}

			return IsMatchAnyArityCore(fullName, symbol, length);
		}

		public static bool ImplementsAnInterface(this ISymbol symbol)
		{
			switch (symbol.Kind)
			{
				case SymbolKind.Method:
				case SymbolKind.Property:
				case SymbolKind.Event:
					break;

				default:
					return false;
			}

			var decl = symbol.ContainingType;

			switch (decl.TypeKind)
			{
				case TypeKind.Interface:
				case TypeKind.Enum:
				case TypeKind.Delegate:
				case TypeKind.Error:
				case TypeKind.Pointer:
					return false;
			}

			foreach (var iface in decl.AllInterfaces)
			{
				foreach (var member in iface.GetMembers(symbol.Name))
				{
					if (member.Kind == symbol.Kind)
					{
						var local = decl.FindImplementationForInterfaceMember(member);

						if (local == symbol)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public static bool IsExternallyVisible(this ISymbol symbol)
		{
			do
			{
				switch (symbol.DeclaredAccessibility)
				{
					case Accessibility.Public:
					case Accessibility.Protected:
					case Accessibility.ProtectedOrInternal:
						break;

					default:
						return false;
				}

				symbol = symbol.ContainingSymbol;

				if (symbol.Kind == SymbolKind.Namespace)
				{
					break;
				}
			}
			while (symbol != null);

			return true;
		}

		static bool IsMatchCore(ISymbol symbol, string fullName, int length)
		{
			while (true)
			{
				var index = fullName.LastIndexOf('.', length - 1);

				if (index < 0)
				{
					break;
				}

				if (!MatchesSubstring(symbol.MetadataName, fullName, index + 1, length))
				{
					return false;
				}

				length = index;
				var ns = symbol.ContainingNamespace;

				if (ns.IsGlobalNamespace)
				{
					return false;
				}

				symbol = ns;
			}

			if (!MatchesSubstring(symbol.MetadataName, fullName, 0, length))
			{
				return false;
			}

			var tmp = symbol.ContainingNamespace;

			return tmp.IsGlobalNamespace;
		}

		static bool IsMatchAnyArityCore(string fullName, ISymbol symbol, int length)
		{
			while (true)
			{
				var index = fullName.LastIndexOf('.', length - 1);

				if (index < 0)
				{
					break;
				}

				if (!MatchesSubstring(symbol.Name, fullName, index + 1, length))
				{
					return false;
				}

				length = index;
				var ns = symbol.ContainingNamespace;

				if (ns.IsGlobalNamespace)
				{
					return false;
				}

				symbol = ns;
			}

			if (!MatchesSubstring(symbol.Name, fullName, 0, length))
			{
				return false;
			}

			var tmp = symbol.ContainingNamespace;

			return tmp.IsGlobalNamespace;
		}

		static bool MatchesSubstring(string name, string fullName, int start, int end)
		{
			var segLen = end - start;
			return name.Length == segLen &&
				string.Compare(name, 0, fullName, start, segLen, StringComparison.Ordinal) == 0;
		}
	}
}
