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
using Microsoft.CodeAnalysis.Formatting;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DiscardThrowCodeFixProvider))]
	[Shared]
	public sealed class DiscardThrowCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.AvoidDiscardCoalesceThrowDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Replace with an if-throw check.",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "ReplaceWithIfThrowCheck"),
				diagnostic: diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (ExpressionStatementSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(node, CreateIfThrowStatement(node)));
		}

		static SyntaxNode CreateIfThrowStatement(ExpressionStatementSyntax node)
		{
			var assignment = (AssignmentExpressionSyntax)node.Expression;
			var coalesce = (BinaryExpressionSyntax)assignment.Right;
			var throwExpression = (ThrowExpressionSyntax)coalesce.Right;

			return SyntaxFactory.IfStatement(
				SyntaxFactory.BinaryExpression(
					SyntaxKind.EqualsExpression,
					coalesce.Left.WithoutTrivia(),
					SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
				SyntaxFactory.Block(
					SyntaxFactory.ThrowStatement(
						throwExpression.ThrowKeyword,
						throwExpression.Expression,
						node.SemicolonToken)))
				.WithLeadingTrivia(node.GetLeadingTrivia())
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
				.WithAdditionalAnnotations(Formatter.Annotation);
		}
	}
}
