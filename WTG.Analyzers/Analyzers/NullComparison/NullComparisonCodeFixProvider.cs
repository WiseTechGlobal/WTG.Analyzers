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
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullComparisonCodeFixProvider))]
	public sealed class NullComparisonCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontEquateValueTypesWithNullDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => NullComparisonFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Simplify",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "Simplify"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = (BinaryExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var value = node.Kind() switch
			{
				SyntaxKind.EqualsExpression => false,
				SyntaxKind.NotEqualsExpression => true,
				_ => throw new InvalidOperationException("Unreachable - encountered unexpected syntax node " + node.Kind()),
			};

			return document.WithSyntaxRoot(ExpressionRemover.ReplaceWithConstantBool(root, node, value));
		}
	}
}
