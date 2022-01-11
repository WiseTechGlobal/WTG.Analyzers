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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LinqCodeFixProvider))]
	[Shared]
	public sealed class LinqCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.PreferDirectMemberAccessOverLinqDiagnosticID,
			Rules.PreferDirectMemberAccessOverLinqInAnExpressionDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => LinqFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			var equivalenceKey = diagnostic.Id switch
			{
				Rules.PreferDirectMemberAccessOverLinqDiagnosticID => "ReplaceWithCorrectPreferredReference",
				Rules.PreferDirectMemberAccessOverLinqInAnExpressionDiagnosticID => "ReplaceWithCorrectPreferredReferenceInAnExpression",
				_ => null,
			};

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Replace with correct preferred reference.",
					createChangedDocument: c => ReplaceWithMemberReference(context.Document, diagnostic, c),
					equivalenceKey: equivalenceKey),
				diagnostic: diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> ReplaceWithMemberReference(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
			var tree = await document.RequireSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
			var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var model = compilation!.GetSemanticModel(tree);

			var invoke = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invoke,
					UpdateInvoke(model, (InvocationExpressionSyntax)invoke)));
		}

		static ExpressionSyntax UpdateInvoke(SemanticModel model, InvocationExpressionSyntax invoke)
		{
			return LinqUtils.GetResolution(model, invoke)!.ApplyFix(invoke);
		}
	}
}
