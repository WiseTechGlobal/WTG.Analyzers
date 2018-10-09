using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArrayCodeFixProvider))]
	[Shared]
	public sealed class ArrayCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Rules.PreferArrayEmptyOverNewArrayConstructionDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.PreferArrayEmptyOverNewArrayConstructionDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Change to Array.Empty<T>()",
							createChangedDocument: c => ReplaceWithArrayEmpty(context.Document, diagnostic, c),
							equivalenceKey: "ChangeToArrayEmptyT"),
						diagnostic: diagnostic);
					break;
			}

			return Task.CompletedTask;
		}

		static async Task<Document> ReplaceWithArrayEmpty(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			ArrayTypeSyntax typeSyntaxNode;

			switch (node.Kind())
			{
				case SyntaxKind.ArrayCreationExpression:
				{
					var syntax = (ArrayCreationExpressionSyntax)node;
					typeSyntaxNode = syntax.Type;
					break;
				}

				case SyntaxKind.ArrayInitializerExpression:
				{
					var syntax = (InitializerExpressionSyntax)node;
					typeSyntaxNode = (ArrayTypeSyntax)node.FirstAncestorOrSelf<VariableDeclarationSyntax>().Type;
					break;
				}

				default:
					return document;
			}

			var arrayType = GetArrayElementTypeSyntax(typeSyntaxNode);

			document = document.WithSyntaxRoot(
				root.ReplaceNode(
					node,
					CreateArrayEmptyInvocation(
						arrayType.WithoutTrivia())
						.WithTriviaFrom(node)));

			return document;
		}

		static TypeSyntax GetArrayElementTypeSyntax(ArrayTypeSyntax syntax)
		{
			if (syntax.RankSpecifiers.Count == 0)
			{
				return syntax.ElementType;
			}
			else
			{
				var specifiers = syntax.RankSpecifiers.RemoveAt(0);
				var arrayType = SyntaxFactory.ArrayType(syntax.ElementType).WithRankSpecifiers(specifiers);
				return arrayType;
			}
		}

		static InvocationExpressionSyntax CreateArrayEmptyInvocation(TypeSyntax elementType)
		{
			return SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.IdentifierName("System"),
						SyntaxFactory.IdentifierName("Array"))
					.WithAdditionalAnnotations(Simplifier.Annotation),
					SyntaxFactory.GenericName(
						SyntaxFactory.Identifier("Empty"))
					.WithTypeArgumentList(
						SyntaxFactory.TypeArgumentList(
							SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
								elementType)))));
		}
	}
}
