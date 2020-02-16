using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VarCodeFixProvider))]
	[Shared]
	public sealed class VarCodeFixProvider : CodeFixProvider
	{
		public const string ChangeToVarEquivalenceKey = "ChangeToVar";
		public const string ChangeToOutVarEquivalenceKey = "ChangeToOutVar";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UseVarWherePossibleDiagnosticID,
			Rules.UseOutVarWherePossibleDiagnosticID,
			Rules.DeconstructWithVarDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => VarFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.DeconstructWithVarDiagnosticID:
				case Rules.UseVarWherePossibleDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Change to var",
							createChangedDocument: c => ReplaceWithVar(context.Document, diagnostic, c),
							equivalenceKey: ChangeToVarEquivalenceKey),
						diagnostic: diagnostic);
					break;

				case Rules.UseOutVarWherePossibleDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Change to var",
							createChangedDocument: c => ReplaceWithVar(context.Document, diagnostic, c),
							equivalenceKey: ChangeToOutVarEquivalenceKey),
						diagnostic: diagnostic);
					break;
			}

			return Task.FromResult<object>(null);
		}

		static async Task<Document> ReplaceWithVar(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					node,
					SyntaxFactory.IdentifierName("var").WithTriviaFrom(node)));
		}
	}
}
