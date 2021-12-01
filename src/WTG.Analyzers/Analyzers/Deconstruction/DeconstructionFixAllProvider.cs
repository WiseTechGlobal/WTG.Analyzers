using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class DeconstructionFixAllProvider : DocumentBatchedFixAllProvider
	{
		DeconstructionFixAllProvider()
		{
		}
		public static DeconstructionFixAllProvider Instance { get; } = new DeconstructionFixAllProvider();

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var syntaxRoot = await originalDocument.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticsToFix = GetDiagnosticsToFix(diagnostics);
			var nodesToFix = diagnosticsToFix.Select(d => syntaxRoot.FindNode(d.Location.SourceSpan))
				.Where(d => d.IsKind(SyntaxKind.TupleExpression))
				.Cast<TupleExpressionSyntax>();

			var newRoot = syntaxRoot.ReplaceNodes(nodesToFix, (originalNode, _) => DeconstructionHelpers.UnifyVarsInTupleExpression(originalNode));
			return documentToFix.WithSyntaxRoot(newRoot);
		}

		static IEnumerable<Diagnostic> GetDiagnosticsToFix(ImmutableArray<Diagnostic> diagnostics)
		{
			var redundantInnerDiagnostics = new HashSet<Diagnostic>();

			foreach (var outer in diagnostics)
			{
				foreach (var inner in diagnostics)
				{
					if (inner != outer)
					{
						var innerSpan = inner.Location.SourceSpan;
						var outerSpan = outer.Location.SourceSpan;

						if (innerSpan.Start > outerSpan.Start && innerSpan.End < outerSpan.End)
						{
							redundantInnerDiagnostics.Add(inner);
						}
					}
				}
			}

			foreach (var diagnostic in diagnostics)
			{
				if (redundantInnerDiagnostics.Contains(diagnostic))
				{
					continue;
				}

				yield return diagnostic;
			}
		}
	}
}
