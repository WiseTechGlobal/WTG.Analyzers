using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers
{
	sealed class SuppressionTargetLookup
	{
		public SuppressionTargetLookup(SemanticModel model)
		{
			this.model = model;
			namespaceCache = new Dictionary<string, bool>();
			typeCache = new Dictionary<string, bool>();
			memberCache = new Dictionary<string, HashSet<string>>();
		}

		public bool NamespaceExists(string namespaceID)
		{
			bool result;

			if (!namespaceCache.TryGetValue(namespaceID, out result))
			{
				namespaceCache.Add(namespaceID, result = NamespaceExistsCore(namespaceID));
			}

			return result;
		}

		public bool TypeExists(string typeID)
		{
			bool result;

			if (!typeCache.TryGetValue(typeID, out result))
			{
				typeCache.Add(typeID, result = TypeExistsCore(typeID));
			}

			return result;
		}

		public bool MemberExists(string memberID)
		{
			var index = memberID.IndexOf('#');

			if (index < 1)
			{
				return false;
			}

			var typeID = memberID.Substring(0, index - 1);

			HashSet<string> set;

			if (!memberCache.TryGetValue(typeID, out set))
			{
				memberCache.Add(typeID, set = ConstructMemberSet(typeID, index - 1));
			}

			return set.Contains(memberID);
		}

		bool NamespaceExistsCore(string namespaceID)
		{
			return EnumerateMatchingNamespaces(namespaceID, namespaceID.Length).Any();
		}

		bool TypeExistsCore(string typeID)
		{
			return EnumerateMatchingTypes(typeID, typeID.Length).Any();
		}

		IEnumerable<INamespaceOrTypeSymbol> EnumerateMatchingNamespaces(string id, int length)
		{
			var index = id.LastIndexOf('.', length - 1);

			if (index >= 0)
			{
				return FindChildrenMatchingSubstring(
					EnumerateMatchingNamespaces(id, index),
					SymbolKind.Namespace,
					id,
					index + 1,
					length - index - 1);
			}

			return FindRootMatchingSubstring(SymbolKind.Namespace, id, length);
		}

		IEnumerable<INamespaceOrTypeSymbol> EnumerateMatchingTypes(string id, int length)
		{
			var index = id.LastIndexOf('+', length - 1);

			if (index >= 0)
			{
				return FindChildrenMatchingSubstring(
					EnumerateMatchingTypes(id, index),
					SymbolKind.NamedType,
					id,
					index + 1,
					length - index - 1);
			}

			index = id.LastIndexOf('.', length - 1);

			if (index >= 0)
			{
				return FindChildrenMatchingSubstring(
					EnumerateMatchingNamespaces(id, index),
					SymbolKind.NamedType,
					id,
					index + 1,
					length - index - 1);
			}

			return FindRootMatchingSubstring(SymbolKind.NamedType, id, length);
		}

		HashSet<string> ConstructMemberSet(string id, int length)
		{
			var set = new HashSet<string>();
			StringBuilder builder = null;
			var len = 0;

			foreach (var scope in EnumerateMatchingTypes(id, length))
			{
				foreach (var member in scope.GetMembers())
				{
					if (builder == null)
					{
						builder = new StringBuilder();
						builder.Append(id, 0, length);
						builder.Append(".#");
						len = builder.Length;
					}
					else
					{
						builder.Length = len;
					}

					switch (member.Kind)
					{
						case SymbolKind.Method:
							WriteMethodSymbol(builder, (IMethodSymbol)member);
							break;

						case SymbolKind.Property:
							WritePropertySymbol(builder, (IPropertySymbol)member);
							break;

						case SymbolKind.Event:
							WriteEventSymbol(builder, (IEventSymbol)member);
							break;

						case SymbolKind.Field:
							WriteFieldSymbol(builder, (IFieldSymbol)member);
							break;

						default:
							continue;
					}

					set.Add(builder.ToString());
				}
			}

			return set;
		}

		IEnumerable<INamespaceOrTypeSymbol> FindRootMatchingSubstring(SymbolKind kind, string name, int length)
		{
			foreach (var root in model.LookupNamespacesAndTypes(0))
			{
				if (root.Kind == kind && MatchesSubstring(root, name, 0, length))
				{
					yield return (INamespaceOrTypeSymbol)root;
				}
			}
		}

		static IEnumerable<INamespaceOrTypeSymbol> FindChildrenMatchingSubstring(IEnumerable<INamespaceOrTypeSymbol> scopes, SymbolKind kind, string name, int offset, int length)
		{
			foreach (var parentScope in scopes)
			{
				foreach (var member in parentScope.GetMembers())
				{
					if (member.Kind == kind && MatchesSubstring(member, name, offset, length))
					{
						yield return (INamespaceOrTypeSymbol)member;
					}
				}
			}
		}

		static bool MatchesSubstring(ISymbol symbol, string name, int offset, int length)
		{
			var tmp = symbol.MetadataName;

			return tmp != null
				&& tmp.Length == length
				&& string.Compare(tmp, 0, name, offset, length, StringComparison.Ordinal) == 0;
		}

		static void WriteScope(StringBuilder builder, INamespaceSymbol symbol)
		{
			if (!symbol.ContainingNamespace.IsGlobalNamespace)
			{
				WriteScope(builder, symbol.ContainingNamespace);
				builder.Append('.');
			}

			builder.Append(symbol.MetadataName);
		}

		static void WriteScope(StringBuilder builder, INamedTypeSymbol typeSymbol)
		{
			if (typeSymbol.ContainingType != null)
			{
				WriteScope(builder, typeSymbol.ContainingType);
				builder.Append('+');
			}
			else
			{
				if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
				{
					WriteScope(builder, typeSymbol.ContainingNamespace);
					builder.Append('.');
				}
			}

			builder.Append(typeSymbol.MetadataName);
		}

		static void WriteMethodSymbol(StringBuilder builder, IMethodSymbol methodSymbol)
		{
			WriteMemberSymbolCore(builder, methodSymbol, methodSymbol.ExplicitInterfaceImplementations.FirstOrDefault());

			if (methodSymbol.TypeArguments.Length > 0)
			{
				builder.Append('`');
				builder.Append(methodSymbol.TypeArguments.Length);
			}

			builder.Append('(');
			WriteParameters(builder, methodSymbol.Parameters);
			builder.Append(')');

			if (methodSymbol.MethodKind == MethodKind.Conversion)
			{
				builder.Append(':');
				WriteType(builder, methodSymbol.ReturnType);
			}
		}

		static void WritePropertySymbol(StringBuilder builder, IPropertySymbol propertySymbol)
		{
			WriteMemberSymbolCore(builder, propertySymbol, propertySymbol.ExplicitInterfaceImplementations.FirstOrDefault());
		}

		static void WriteEventSymbol(StringBuilder builder, IEventSymbol eventSymbol)
		{
			WriteMemberSymbolCore(builder, eventSymbol, eventSymbol.ExplicitInterfaceImplementations.FirstOrDefault());
		}

		static void WriteFieldSymbol(StringBuilder builder, IFieldSymbol symbol)
		{
			WriteMemberSymbolCore(builder, symbol, null);
		}

		static void WriteMemberSymbolCore(StringBuilder builder, ISymbol memberSymbol, ISymbol explicitSymbol)
		{
			if (explicitSymbol != null)
			{
				WriteType(builder, explicitSymbol.ContainingType);
				builder.Append('.');
				builder.Append(explicitSymbol.MetadataName);
			}
			else
			{
				builder.Append(memberSymbol.MetadataName);
			}
		}

		static void WriteParameters(StringBuilder builder, IEnumerable<IParameterSymbol> parameters)
		{
			using (var enumerator = parameters.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					WriteParameter(builder, enumerator.Current);

					while (enumerator.MoveNext())
					{
						builder.Append(',');
						WriteParameter(builder, enumerator.Current);
					}
				}
			}
		}

		static void WriteParameter(StringBuilder builder, IParameterSymbol parameter)
		{
			WriteType(builder, parameter.Type);

			if (parameter.RefKind != RefKind.None)
			{
				builder.Append('&');
			}
		}

		static void WriteTypes(StringBuilder builder, IEnumerable<ITypeSymbol> types)
		{
			using (var enumerator = types.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					WriteType(builder, enumerator.Current);

					while (enumerator.MoveNext())
					{
						builder.Append(',');
						WriteType(builder, enumerator.Current);
					}
				}
			}
		}

		static void WriteType(StringBuilder builder, ITypeSymbol type)
		{
			switch (type.Kind)
			{
				case SymbolKind.ErrorType:
				case SymbolKind.NamedType:
					WriteType(builder, (INamedTypeSymbol)type);
					break;

				case SymbolKind.ArrayType:
					WriteType(builder, (IArrayTypeSymbol)type);
					break;

				case SymbolKind.TypeParameter:
					WriteType(builder, (ITypeParameterSymbol)type);
					break;

				default:
					throw new ArgumentException("Unrecognised symbol type: " + type.Kind, nameof(type));
			}
		}

		static void WriteType(StringBuilder builder, INamedTypeSymbol type)
		{
			WriteScope(builder, type);

			if (type.TypeArguments.Length > 0)
			{
				builder.Append('<');
				WriteTypes(builder, type.TypeArguments);
				builder.Append('>');
			}
		}

		static void WriteType(StringBuilder builder, IArrayTypeSymbol type)
		{
			WriteType(builder, type.ElementType);
			builder.Append('[');
			builder.Append(',', type.Rank - 1);
			builder.Append(']');
		}

		static void WriteType(StringBuilder builder, ITypeParameterSymbol type)
		{
			if (type.TypeParameterKind == TypeParameterKind.Method)
			{
				builder.Append("!!");
			}
			else
			{
				builder.Append('!');
			}

			builder.Append(type.Ordinal);
		}

		readonly SemanticModel model;
		readonly Dictionary<string, bool> namespaceCache;
		readonly Dictionary<string, bool> typeCache;
		readonly Dictionary<string, HashSet<string>> memberCache;
	}
}
