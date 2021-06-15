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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BooleanLiteralCombiningCodeFixProvider))]
	[Shared]
	public sealed class BooleanLiteralCombiningCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.AvoidBoolLiteralsInLargerBoolExpressionsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => BooleanLiteralCombiningFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Simplify.",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "SimplifyCombinedBoolLiteral"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var literal = (LiteralExpressionSyntax)root.FindNode(diagnosticSpan);

			return document.WithSyntaxRoot(
				ExpressionRemover.ReplaceWithConstantBool(root, literal, literal.IsKind(SyntaxKind.TrueLiteralExpression)));
		}
	}
}
