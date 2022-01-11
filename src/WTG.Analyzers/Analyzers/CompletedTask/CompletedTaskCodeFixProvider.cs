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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CompletedTaskCodeFixProvider))]
	[Shared]
	public sealed class CompletedTaskCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.PreferCompletedTaskDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Replace with `Task.CompletedTask`.",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "ChangeToTaskCompletedTask"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(node, CreateCompletedTask().WithTriviaFrom(node)));
		}

		static ExpressionSyntax CreateCompletedTask()
		{
			// System.Threading.Tasks.Task
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.IdentifierName("System"),
							SyntaxFactory.IdentifierName("Threading")),
						SyntaxFactory.IdentifierName("Tasks")),
					SyntaxFactory.IdentifierName("Task")),
				SyntaxFactory.IdentifierName("CompletedTask"))
				.WithAdditionalAnnotations(Simplifier.Annotation);
		}
	}
}
