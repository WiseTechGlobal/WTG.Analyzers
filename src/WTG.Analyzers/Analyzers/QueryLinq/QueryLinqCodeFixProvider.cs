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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QueryLinqCodeFixProvider))]
	[Shared]
	public sealed class QueryLinqCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UsingEnumerableExtensionMethodsOnAQueryableDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Add AsEnumerable()",
					createChangedDocument: c => AddAsEnumerable(context.Document, diagnostic, c),
					equivalenceKey: "AddAsEnumerable"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> AddAsEnumerable(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			var invocationExpressionNode = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
			if (invocationExpressionNode?.Expression is not MemberAccessExpressionSyntax memberAccessExpressionNode)
			{
				return document;
			}

			var newNode = invocationExpressionNode.WithExpression(MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							InvocationExpression(
								MemberAccessExpression(
									SyntaxKind.SimpleMemberAccessExpression,
									memberAccessExpressionNode.Expression,
									IdentifierName("AsEnumerable"))),
							memberAccessExpressionNode.Name))
					.WithArgumentList(invocationExpressionNode.ArgumentList)
					.NormalizeWhitespace();

			var newRoot = root.ReplaceNode(invocationExpressionNode, newNode);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
