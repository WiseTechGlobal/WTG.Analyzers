using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class CodeContractsFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static CodeContractsFixAllProvider Instance { get; } = new CodeContractsFixAllProvider();

		CodeContractsFixAllProvider()
		{
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await originalDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var lookup = new Dictionary<SyntaxNode, Diagnostic>();

			foreach (var diagnostic in diagnostics)
			{
				var node = root.FindNode(diagnostic.Location.SourceSpan);
				lookup[node] = diagnostic;
			}

			root = root.ReplaceNodes(lookup.Keys, ComputeReplacement);
			root = root.RemoveNodes(
				root.GetAnnotatedNodes(DeleteMEAnnotation),
				SyntaxRemoveOptions.AddElasticMarker | SyntaxRemoveOptions.KeepExteriorTrivia);

			return documentToFix
				.WithSyntaxRoot(CodeContractsUsingSimplifier.Instance.Visit(
					root));

			SyntaxNode ComputeReplacement(SyntaxNode original, SyntaxNode modified)
			{
				if (lookup.TryGetValue(original, out var diagnostic) &&
					diagnostic.Properties.TryGetValue(CodeContractsAnalyzer.PropertyProposedFix, out var fix))
				{
					if (fix == CodeContractsAnalyzer.FixDelete)
					{
						return modified.WithAdditionalAnnotations(DeleteMEAnnotation);
					}
					else if (fix == CodeContractsAnalyzer.FixRequires)
					{
						var statement = (ExpressionStatementSyntax)original;
						var invoke = (InvocationExpressionSyntax)statement.Expression;

						if (CodeContractsHelper.IsGenericMethod(invoke, out var location))
						{
							modified = CodeContractsHelper.ConvertGenericRequires(invoke, location);
						}
						else if (CodeContractsHelper.IsInPrivateMember(semanticModel, statement, cancellationToken))
						{
							return modified.WithAdditionalAnnotations(DeleteMEAnnotation);
						}
						else if (CodeContractsHelper.IsNullArgumentCheck(semanticModel, invoke, out location, cancellationToken))
						{
							modified = CodeContractsHelper.ConvertRequiresNotNull(invoke, location);
						}
						else if (CodeContractsHelper.IsNonEmptyStringArgumentCheck(semanticModel, invoke, out location, cancellationToken))
						{
							modified = CodeContractsHelper.ConvertRequires(invoke, location, "Value cannot be null or empty.");
						}
						else if (CodeContractsHelper.InvokesContractForAll(semanticModel, invoke, CancellationToken.None))
						{
							return modified.WithAdditionalAnnotations(DeleteMEAnnotation);
						}
						else if (CodeContractsHelper.AccessesParameter(semanticModel, invoke, out location, cancellationToken))
						{
							modified = CodeContractsHelper.ConvertRequires(invoke, location, "Invalid Argument.");
						}
						else
						{
							return modified;
						}

						return CodeContractsHelper.WithElasticTriviaFrom(modified, original);
					}
				}

				return modified;
			}
		}

		static readonly SyntaxAnnotation DeleteMEAnnotation = new SyntaxAnnotation(nameof(DeleteMEAnnotation));
	}
}
