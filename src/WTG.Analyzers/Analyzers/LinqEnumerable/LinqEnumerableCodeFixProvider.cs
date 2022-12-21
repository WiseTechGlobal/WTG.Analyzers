using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Analyzers.LinqEnumerable;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LinqEnumerableCodeFixProvider))]
	[Shared]
	public class LinqEnumerableCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID,
			Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID,
			Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			var equivalenceKey = diagnostic.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => "Append",
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => "Prepend",
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => "Join",
				_ => null,
			};

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Fix incorrect use of .Concat",
					createChangedDocument: c => ReplaceWithAppropriateMethod(context.Document, diagnostic, c),
					equivalenceKey: equivalenceKey),
				diagnostic);

			return Task.CompletedTask;
		}

		public static async Task<Document> ReplaceWithAppropriateMethod(Document document, Diagnostic diagnostic, CancellationToken c)
		{
			var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(true);

			if (root == null)
			{
				return document;
			}

			var memberAccessExpression = (MemberAccessExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			var newNode = FixMemberAccessExpression(memberAccessExpression, diagnostic);

			if (newNode == null)
			{
				return document;
			}

			return document.WithSyntaxRoot(root.ReplaceNode(
				memberAccessExpression.Parent!, newNode));
		}

		public static SyntaxNode? FixMemberAccessExpression(MemberAccessExpressionSyntax m, Diagnostic d)
		{
			return d.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => LinqEnumerableUtils.FixConcatWithAppendMethod(m),
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => LinqEnumerableUtils.FixConcatWithPrependMethod(m),
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => LinqEnumerableUtils.FixConcatWithNewCollection(m),
				_ => null,
			};
		}
	}
}
