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
			return node.Kind() switch
			{
				SyntaxKind.CompilationUnit => ((CompilationUnitSyntax)node).Usings,
				SyntaxKind.NamespaceDeclaration => ((NamespaceDeclarationSyntax)node).Usings,
				_ => throw new ArgumentException("Syntax Node is not known to contain usings: " + node.GetType().Name, nameof(node)),
			};
		}

		public static SyntaxNode WithUsings(SyntaxNode node, SyntaxList<UsingDirectiveSyntax> usings)
		{
			return node.Kind() switch
			{
				SyntaxKind.CompilationUnit => ((CompilationUnitSyntax)node).WithUsings(usings),
				SyntaxKind.NamespaceDeclaration => ((NamespaceDeclarationSyntax)node).WithUsings(usings),
				_ => throw new ArgumentException("Syntax Node is not known to contain usings: " + node.GetType().Name, nameof(node)),
			};
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
