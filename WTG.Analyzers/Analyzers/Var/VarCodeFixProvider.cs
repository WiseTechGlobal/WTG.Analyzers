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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VarCodeFixProvider)), Shared]
	public sealed class VarCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UseVarWherePossibleDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Change to var",
					createChangedDocument: c => ReplaceWithVar(context.Document, diagnostic, c),
					equivalenceKey: "ChangeToVar"),
				diagnostic: diagnostic);

			return Task.FromResult<object>(null);
		}

		async Task<Document> ReplaceWithVar(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);

			var newNode = VarTypeSyntax
				.WithLeadingTrivia(node.GetLeadingTrivia())
				.WithTrailingTrivia(node.GetTrailingTrivia());

			var newRoot = root.ReplaceNode(node, newNode);
			return document.WithSyntaxRoot(newRoot);
		}

		static readonly TypeSyntax VarTypeSyntax = SyntaxFactory.ParseTypeName("var");
	}
}
