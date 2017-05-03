using System;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class SyntaxTreeExtensions
	{
		public static bool IsGenerated(this SyntaxTree tree, CancellationToken token)
		{
			if (tree == null)
			{
				throw new ArgumentNullException(nameof(tree));
			}

			// It would be nice if roslyn provided this information for us, but I wouldn't hold your breath.
			// https://github.com/dotnet/roslyn/issues/3705
			return FilenameLooksGenerated(tree.FilePath) || ContainsGeneratedCodeComment(tree, token);
		}

		static bool FilenameLooksGenerated(string filename)
		{
			return filename != null
				&& (filename.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase));
		}

		static bool ContainsGeneratedCodeComment(this SyntaxTree tree, CancellationToken token)
		{
			var root = (CompilationUnitSyntax)tree.GetRoot(token);

			foreach (var trivia in root.DescendantTrivia(NotMembers))
			{
				if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia &&
					IsGeneratedCommentMarkerRegex.IsMatch(trivia.ToString()))
				{
					return true;
				}
			}

			return false;
		}

		static bool NotMembers(SyntaxNode arg)
		{
			switch (arg.Kind())
			{
				case SyntaxKind.AttributeList:
				case SyntaxKind.CompilationUnit:
				case SyntaxKind.ExternAliasDirective:
				case SyntaxKind.UsingDirective:
				case SyntaxKind.NamespaceDeclaration:
				case SyntaxKind.ClassDeclaration:
					return true;
			}

			return false;
		}

		static readonly Regex IsGeneratedCommentMarkerRegex = new Regex(@"^//\s*<auto-?generated\s*/?>\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
	}
}
