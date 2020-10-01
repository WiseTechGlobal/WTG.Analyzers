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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BooleanLiteralCodeFixProvider))]
	[Shared]
	public sealed class BooleanLiteralCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UseNamedArgumentsWhenPassingBooleanLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Use Named Arguments",
					createChangedDocument: c => Fix(context.Document, diagnostic, c),
					equivalenceKey: "UseNamedArguments"),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> Fix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var literal = (LiteralExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var argument = (ArgumentSyntax)literal.Parent;
			var argumentList = (ArgumentListSyntax)argument.Parent;
			var index = argumentList.FindIndexOfArgument(argument);

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (argumentList.TryFindCorrespondingParameterSymbol(index, semanticModel, cancellationToken) is { } argumentSymbol)
			{
				root = root.ReplaceNode(
					argument,
					argument.WithNameColon(
						SyntaxFactory.NameColon(
							SyntaxFactory.IdentifierName(argumentSymbol.Name)))
						.WithTriviaFrom(argument));

				document = document.WithSyntaxRoot(root);
			}

			return document;
		}
	}
}
