using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed partial class ModifierExtractionVisitor : CSharpSyntaxVisitor<SyntaxTokenList>
	{
		public static ModifierExtractionVisitor Instance { get; } = new ModifierExtractionVisitor();

		public static ImmutableArray<SyntaxKind> SupportedSyntaxKinds { get; } = ImmutableArray.Create(
			SyntaxKind.ClassDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.ConversionOperatorDeclaration,
			SyntaxKind.DelegateDeclaration,
			SyntaxKind.DestructorDeclaration,
			SyntaxKind.EnumDeclaration,
			SyntaxKind.EventDeclaration,
			SyntaxKind.EventFieldDeclaration,
			SyntaxKind.FieldDeclaration,
			SyntaxKind.IncompleteMember,
			SyntaxKind.IndexerDeclaration,
			SyntaxKind.InterfaceDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.OperatorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.StructDeclaration);

		ModifierExtractionVisitor()
		{
		}

		public override SyntaxTokenList VisitClassDeclaration(ClassDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitDelegateDeclaration(DelegateDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitDestructorDeclaration(DestructorDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitEnumDeclaration(EnumDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitEventDeclaration(EventDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitFieldDeclaration(FieldDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitIncompleteMember(IncompleteMemberSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitIndexerDeclaration(IndexerDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitMethodDeclaration(MethodDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitOperatorDeclaration(OperatorDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitPropertyDeclaration(PropertyDeclarationSyntax node) => node.Modifiers;
		public override SyntaxTokenList VisitStructDeclaration(StructDeclarationSyntax node) => node.Modifiers;

		public override SyntaxTokenList DefaultVisit(SyntaxNode node)
		{
			throw new ArgumentException("Unsupported SyntaxNode type: " + node.Kind(), nameof(node));
		}
	}
}
