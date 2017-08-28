using System;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DeconstructionCodeFixProvider))]
	[Shared]
	public sealed class DeconstructionCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DeconstructWithSingleVarDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => DeconstructionFixAllProvider.Instance;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.DeconstructWithSingleVarDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Move var outside of the deconstruction.",
							createChangedDocument: c => MoveVarOutsideDeconstructionsAsync(context.Document, diagnostic, c),
							equivalenceKey: "SingleVarWhenDeconstructing"),
						diagnostic);
					break;
			}

			return Task.FromResult<object>(null);
		}

		static async Task<Document> MoveVarOutsideDeconstructionsAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			if (node.IsKind(SyntaxKind.TupleExpression))
			{
				var tupleExpression = (TupleExpressionSyntax)node;
				var newNode = DeconstructionHelpers.UnifyVarsInTupleExpression(tupleExpression);

				var documentToFixRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
				var newRoot = documentToFixRoot.ReplaceNode(node, newNode);
				document = document.WithSyntaxRoot(newRoot);
			}

			return document;
		}
	}
}
