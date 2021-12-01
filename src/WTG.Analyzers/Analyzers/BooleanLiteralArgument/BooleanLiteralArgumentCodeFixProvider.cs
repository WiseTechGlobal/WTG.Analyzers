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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BooleanLiteralArgumentCodeFixProvider))]
	[Shared]
	public sealed class BooleanLiteralArgumentCodeFixProvider : CodeFixProvider
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
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var literal = (LiteralExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			var argument = (ArgumentSyntax?)literal.Parent;
			NRT.Assert(argument != null, "The fixer should only be running on a full and complete document.");
			var argumentList = (ArgumentListSyntax?)argument.Parent;
			NRT.Assert(argumentList != null, "The fixer should only be running on a full and complete document.");
			var index = argumentList.Arguments.IndexOf(argument);

			var semanticModel = await document.RequireSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (argumentList.TryFindCorrespondingParameterSymbol(semanticModel, index, cancellationToken) is { } argumentSymbol)
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
