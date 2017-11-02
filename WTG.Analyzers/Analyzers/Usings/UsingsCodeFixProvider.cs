using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingsCodeFixProvider))]
	public sealed class UsingsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UsingDirectivesMustBeOrderedByKindDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var document = context.Document;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Rearrange Usings",
					createChangedDocument: c => FixBySortingUsingsAsync(diagnostic, document, c),
					equivalenceKey: "RearrangeUsingsByKind"),
				diagnostic);

			return Task.FromResult<object>(null);
		}

		static async Task<Document> FixBySortingUsingsAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);
			var parent = node.Parent;

			var usings = UsingsHelper.ExtractUsings(parent);
			usings = SortUsings(usings);
			var newParent = UsingsHelper.WithUsings(parent, usings);

			var newRoot = root.ReplaceNode(node.Parent, newParent);
			return document.WithSyntaxRoot(newRoot);
		}

		static SyntaxList<UsingDirectiveSyntax> SortUsings(SyntaxList<UsingDirectiveSyntax> unsorted)
		{
			var regularUsings = new List<UsingDirectiveSyntax>();
			var staticUsings = new List<UsingDirectiveSyntax>();
			var aliasedUsings = new List<UsingDirectiveSyntax>();

			foreach (var syntax in unsorted)
			{
				switch (UsingsHelper.GetUsingDirectiveKind(syntax))
				{
					case UsingDirectiveKind.Regular:
						regularUsings.Add(syntax);
						break;

					case UsingDirectiveKind.Static:
						staticUsings.Add(syntax);
						break;

					case UsingDirectiveKind.Alias:
						aliasedUsings.Add(syntax);
						break;
				}
			}

			var sorted = new SyntaxList<UsingDirectiveSyntax>()
				.AddRange(regularUsings)
				.AddRange(staticUsings)
				.AddRange(aliasedUsings);
			return sorted;
		}
	}
}
