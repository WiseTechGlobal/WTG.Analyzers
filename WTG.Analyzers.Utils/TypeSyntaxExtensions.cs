using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class TypeSyntaxExtensions
	{
		public static bool IsMatch(this TypeSyntax syntax, string name) => IsMatch(syntax, name, name.Length);

		static bool IsMatch(TypeSyntax syntax, string name, int length)
		{
			switch (syntax.Kind())
			{
				case SyntaxKind.IdentifierName:
					{
						var identifier = ((IdentifierNameSyntax)syntax).Identifier.Text;
						return identifier.Length == length && name.StartsWith(identifier, StringComparison.Ordinal);
					}

				case SyntaxKind.QualifiedName:
					{
						var qualifiedNameSyntax = (QualifiedNameSyntax)syntax;
						var identifier = qualifiedNameSyntax.Right.Identifier.Text;
						return length > identifier.Length
							&& string.CompareOrdinal(name, length - identifier.Length, identifier, 0, identifier.Length) == 0
							&& name[length - identifier.Length - 1] == '.'
							&& IsMatch(qualifiedNameSyntax.Left, name, length - identifier.Length - 1);
					}

				case SyntaxKind.AliasQualifiedName:
					return IsMatch(((AliasQualifiedNameSyntax)syntax).Name, name, length);
			}

			return false;
		}
	}
}
