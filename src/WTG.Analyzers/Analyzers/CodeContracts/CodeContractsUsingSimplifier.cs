using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class CodeContractsUsingSimplifier : CSharpSyntaxRewriter
	{
		public static CodeContractsUsingSimplifier Instance { get; } = new CodeContractsUsingSimplifier();

		CodeContractsUsingSimplifier()
		{
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) => node;
		public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => node;
		public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node) => node;
		public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node) => node;
		public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node) => node;
		public override SyntaxNode VisitAttributeList(AttributeListSyntax node) => node;

		public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
		{
			if (!node.HasAnnotation(Simplifier.Annotation) && (node.Name.IsMatch("System.Diagnostics.Contracts") || node.Name.IsMatch("System.Diagnostics.CodeAnalysis")))
			{
				return node.WithAdditionalAnnotations(Simplifier.Annotation);
			}

			return node;
		}
	}
}
