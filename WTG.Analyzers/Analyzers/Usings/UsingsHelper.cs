using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class UsingsHelper
	{
		public static SyntaxList<UsingDirectiveSyntax> ExtractUsings(SyntaxNode node)
		{
			switch (node.Kind())
			{
				case SyntaxKind.CompilationUnit:
					return ((CompilationUnitSyntax)node).Usings;

				case SyntaxKind.NamespaceDeclaration:
					return ((NamespaceDeclarationSyntax)node).Usings;

				default:
					throw new ArgumentException("Syntax Node is not known to contain usings: " + node.GetType().Name, nameof(node));
			}
		}

		public static SyntaxNode WithUsings(SyntaxNode node, SyntaxList<UsingDirectiveSyntax> usings)
		{
			switch (node.Kind())
			{
				case SyntaxKind.CompilationUnit:
					return ((CompilationUnitSyntax)node).WithUsings(usings);

				case SyntaxKind.NamespaceDeclaration:
					return ((NamespaceDeclarationSyntax)node).WithUsings(usings);

				default:
					throw new ArgumentException("Syntax Node is not known to contain usings: " + node.GetType().Name, nameof(node));
			}
		}

		public static UsingDirectiveKind GetUsingDirectiveKind(UsingDirectiveSyntax syntax)
		{
			if (syntax.Alias != null)
			{
				return UsingDirectiveKind.Alias;
			}

			if (syntax.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
			{
				return UsingDirectiveKind.Static;
			}

			return UsingDirectiveKind.Regular;
		}
	}
}
