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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitCompletedCodeFixProvider))]
	[Shared]
	public sealed class AwaitCompletedCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontAwaitTriviallyCompletedTasksDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			if (!diagnostic.Properties.TryGetValue(AwaitCompletedAnalyzer.SourcePropertyName, out var source))
			{
				return Task.CompletedTask;
			}

			switch (source)
			{
				case nameof(Task.CompletedTask):
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Remove",
							createChangedDocument: c => RemoveAwaitCompletedTaskAsync(context.Document, diagnostic, c),
							equivalenceKey: "RemoveAwait"),
						diagnostic: diagnostic);
					break;

				case nameof(Task.FromResult):
					if (diagnostic.AdditionalLocations.Count == 1)
					{
						context.RegisterCodeFix(
							CodeAction.Create(
								title: "Unwrap value.",
								createChangedDocument: c => UnwrapValueAsync(context.Document, diagnostic, c),
								equivalenceKey: "RemoveAwait"),
							diagnostic: diagnostic);
					}
					break;

				case nameof(Task.FromException):
					if (diagnostic.AdditionalLocations.Count == 1)
					{
						context.RegisterCodeFix(
							CodeAction.Create(
								title: "Unwrap throw.",
								createChangedDocument: c => UnwrapThrowAsync(context.Document, diagnostic, c),
								equivalenceKey: "RemoveAwait"),
							diagnostic: diagnostic);
					}
					break;
			}

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveAwaitCompletedTaskAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (AwaitExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var newRoot = root.RemoveNode(
				node.Parent.IsKind(SyntaxKind.ExpressionStatement)
					? node.Parent
					: node,
				SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.AddElasticMarker);

			NRT.Assert(newRoot != null, "We only remove an expression, not the entire document.");

			return document.WithSyntaxRoot(newRoot);
		}

		static async Task<Document> UnwrapValueAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (AwaitExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var replacementNode = node.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					node,
					replacementNode));
		}

		static async Task<Document> UnwrapThrowAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
			NRT.Assert(node.Parent != null, "The fixer should only be running on a full and complete document.");
			var exceptionNode = (ExpressionSyntax)node.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true).WithoutTrivia();

			SyntaxNode replacementNode;

			switch (node.Parent.Kind())
			{
				case SyntaxKind.ExpressionStatement:
				case SyntaxKind.ReturnStatement:
					node = node.Parent;
					replacementNode = SyntaxFactory.ThrowStatement(exceptionNode);
					break;

				default:
					replacementNode = SyntaxFactory.ThrowExpression(exceptionNode);
					break;
			}

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					node,
					replacementNode.WithTriviaFrom(node)));
		}
	}
}
