using System;
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

namespace WTG.Analyzers
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullComparisonCodeFixProvider))]
	public sealed class NullComparisonCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontEquateValueTypesWithNullDiagnosticID);

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

			SyntaxNode literalNode;

			switch (node.Kind())
			{
				case SyntaxKind.EqualsExpression:
					literalNode = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
					break;

				case SyntaxKind.NotEqualsExpression:
					literalNode = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
					break;

				default:
					throw new InvalidOperationException("Unreachable - encountered unexpected syntax node " + node.Kind());
			}

			return document.WithSyntaxRoot(
				root.ReplaceNode(node, literalNode.WithTriviaFrom(node)));
		}
	}
}
