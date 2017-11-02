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
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncCodeFixProvider))]
	[Shared]
	public sealed class BooleanComparisonCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotCompareBoolToAConstantValueDiagnosticID,
			Rules.DoNotCompareBoolToAConstantValueInAnExpressionDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Simplify",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "Simplify"),
				diagnostic: diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (BinaryExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			ExpressionSyntax newNode;
			ExpressionSyntax discardNode;

			if (node.Left.Span.OverlapsWith(diagnosticSpan))
			{
				discardNode = node.Left;
				newNode = node.Right.WithLeadingTrivia(discardNode.GetLeadingTrivia());
			}
			else
			{
				discardNode = node.Right;
				newNode = node.Left.WithTrailingTrivia(discardNode.GetTrailingTrivia());
			}

			var comparand = BoolLiteralVisitor.Instance.Visit(discardNode).Value;
			var isEquality = node.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken);

			if (isEquality != comparand)
			{
				newNode = ExpressionSyntaxFactory.LogicalNot(newNode);
			}

			return document.WithSyntaxRoot(
				root.ReplaceNode(node, newNode));
		}
	}
}
