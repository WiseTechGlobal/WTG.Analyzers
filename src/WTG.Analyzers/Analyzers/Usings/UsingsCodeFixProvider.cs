using System.Collections.Generic;
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

			return Task.CompletedTask;
		}

		static async Task<Document> FixBySortingUsingsAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);
			var parent = node.Parent;
			NRT.Assert(parent != null, "The fixer should only be running on a full and complete document.");

			var usings = UsingsHelper.ExtractUsings(parent);
			usings = SortUsings(usings);
			var newParent = UsingsHelper.WithUsings(parent, usings);

			var newRoot = root.ReplaceNode(parent, newParent);
			return document.WithSyntaxRoot(newRoot);
		}

		static SyntaxList<UsingDirectiveSyntax> SortUsings(SyntaxList<UsingDirectiveSyntax> unsorted)
		{
			var result = new SyntaxList<UsingDirectiveSyntax>();
			var group = new List<UsingDirectiveSyntax>();

			foreach (var syntax in unsorted)
			{
				if (HasConditionalDirectiveTrivia(syntax) && group.Count > 0)
				{
					result = result.AddRange(SortGroup(group));
					group.Clear();
				}

				group.Add(syntax);
			}

			if (group.Count > 0)
			{
				result = result.AddRange(SortGroup(group));
			}

			return result;
		}

		static IEnumerable<UsingDirectiveSyntax> SortGroup(List<UsingDirectiveSyntax> group)
		{
			var regularUsings = new List<UsingDirectiveSyntax>();
			var staticUsings = new List<UsingDirectiveSyntax>();
			var aliasedUsings = new List<UsingDirectiveSyntax>();

			foreach (var syntax in group)
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

			return regularUsings.Concat(staticUsings).Concat(aliasedUsings);
		}

		static bool HasConditionalDirectiveTrivia(UsingDirectiveSyntax node)
		{
			foreach (var trivia in node.GetLeadingTrivia())
			{
				switch (trivia.Kind())
				{
					case SyntaxKind.IfDirectiveTrivia:
					case SyntaxKind.ElifDirectiveTrivia:
					case SyntaxKind.ElseDirectiveTrivia:
					case SyntaxKind.EndIfDirectiveTrivia:
						return true;
				}
			}

			return false;
		}
	}
}
