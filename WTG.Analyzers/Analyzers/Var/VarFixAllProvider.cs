using System.Collections.Generic;
using System.Collections.Immutable;
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
	// We need a special FixAllProvider for the 'out var' case as fixing one diagnostic may invalidate another on the same invoke.
	sealed class VarFixAllProvider : DocumentBatchedFixAllProvider
	{
		public static VarFixAllProvider Instance { get; } = new VarFixAllProvider();

		VarFixAllProvider()
		{
		}

		public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
		{
			if (fixAllContext.CodeActionEquivalenceKey != VarCodeFixProvider.ChangeToOutVarEquivalenceKey)
			{
				// We only want to special-case fixes for 'out var'.
				return WellKnownFixAllProviders.BatchFixer.GetFixAsync(fixAllContext);
			}

			return base.GetFixAsync(fixAllContext);
		}

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.GetSyntaxRootAsync(cancellationToken);
			var model = await originalDocument.GetSemanticModelAsync(cancellationToken);
			var group = new Dictionary<InvocationExpressionSyntax, List<TypeSyntax>>();

			foreach (var diagnostic in diagnostics)
			{
				var type = (TypeSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
				var invoke = type.FirstAncestorOrSelf<InvocationExpressionSyntax>();

				if (!group.TryGetValue(invoke, out var list))
				{
					group.Add(invoke, list = new List<TypeSyntax>());
				}

				list.Add(type);
			}

			return documentToFix.WithSyntaxRoot(
				root.ReplaceNodes(
					group.Keys,
					(originalInvoke, targetInvoke) =>
					{
						return ReplaceTypes(model, originalInvoke, targetInvoke, group[originalInvoke]);
					}));
		}

		static InvocationExpressionSyntax ReplaceTypes(SemanticModel model, InvocationExpressionSyntax originalInvoke, InvocationExpressionSyntax targetInvoke, List<TypeSyntax> types)
		{
			var originalArguments = originalInvoke.ArgumentList.Arguments;
			var index = GetIndex(originalArguments, types[0]);
			targetInvoke = VarifyOutTypeAtIndex(targetInvoke, index);

			if (types.Count > 1)
			{
				// The analyzer has already determined that each individual type can be safely
				// replaced, but did not take into acount combining fixes. So we need to re-evaluate
				// each proposed type replacement after the first.

				var requiredSymbol = (IMethodSymbol)model.GetSymbolInfo(originalInvoke).Symbol;

				for (var i = 1; i < types.Count; i++)
				{
					index = GetIndex(originalArguments, types[i]);
					var proposedInvoke = VarifyOutTypeAtIndex(targetInvoke, index);
					var actualSymbol = (IMethodSymbol)model.GetSpeculativeSymbolInfo(originalInvoke.SpanStart, proposedInvoke, SpeculativeBindingOption.BindAsExpression).Symbol;

					if (requiredSymbol.Equals(actualSymbol))
					{
						targetInvoke = proposedInvoke;
					}
				}
			}

			return targetInvoke;
		}

		static InvocationExpressionSyntax VarifyOutTypeAtIndex(InvocationExpressionSyntax targetInvoke, int index)
		{
			var targetArgumentList = targetInvoke.ArgumentList;
			var targetArguments = targetArgumentList.Arguments;
			var targetArgument = targetArguments[index];
			var targetExpression = (DeclarationExpressionSyntax)targetArgument.Expression;

			return targetInvoke.WithArgumentList(
				targetArgumentList.WithArguments(
					targetArguments.Replace(
						targetArgument,
						targetArgument.WithExpression(
							targetExpression.WithType(
								SyntaxFactory.IdentifierName("var").WithTriviaFrom(targetExpression.Type))))));
		}

		static int GetIndex(SeparatedSyntaxList<ArgumentSyntax> arguments, TypeSyntax type)
		{
			for (var i = 0; i < arguments.Count; i++)
			{
				if (arguments[i].Span.IntersectsWith(type.Span))
				{
					return i;
				}
			}

			return -1;
		}
	}
}
