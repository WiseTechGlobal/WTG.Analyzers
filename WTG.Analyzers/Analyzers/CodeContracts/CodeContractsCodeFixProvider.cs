using System;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeContractsCodeFixProvider))]
	[Shared]
	public sealed class CodeContractsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseCodeContractsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => CodeContractsFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			if (diagnostic.Properties.TryGetValue(CodeContractsAnalyzer.PropertyProposedFix, out var proposedFix))
			{
				switch (proposedFix)
				{
					case CodeContractsAnalyzer.FixDelete:
						context.RegisterCodeFix(CreateDeleteAction(context.Document, diagnostic), diagnostic);
						break;

					case CodeContractsAnalyzer.FixRequires:
						return RegisterCodeFixesForRequiresAsync(context, diagnostic);
				}
			}

			return Task.CompletedTask;
		}

		static async Task RegisterCodeFixesForRequiresAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			var cancellationToken = context.CancellationToken;
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			if (!node.IsKind(SyntaxKind.ExpressionStatement))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)((ExpressionStatementSyntax)node).Expression;

			if (CodeContractsHelper.IsGenericMethod(invoke, out var supplementalLocation))
			{
				context.RegisterCodeFix(
					CreateReplaceWithIfAction(c => FixGenericRequires(document, diagnostic, supplementalLocation, c)),
					diagnostic);
			}
			else
			{
				var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

				if (CodeContractsHelper.IsInPrivateMember(semanticModel, invoke, context.CancellationToken))
				{
					context.RegisterCodeFix(CreateDeleteAction(context.Document, diagnostic), diagnostic);
				}
				else if (CodeContractsHelper.IsNullArgumentCheck(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequiresNotNull(document, diagnostic, supplementalLocation, c)),
						diagnostic);
				}
				else if (CodeContractsHelper.IsNonEmptyStringArgumentCheck(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequires(document, diagnostic, supplementalLocation, "Value cannot be null or empty.", c)),
						diagnostic);
				}
				else if (CodeContractsHelper.InvokesContractForAll(semanticModel, invoke, CancellationToken.None))
				{
					context.RegisterCodeFix(CreateDeleteAction(context.Document, diagnostic), diagnostic);
				}
				else if (CodeContractsHelper.AccessesParameter(semanticModel, invoke, out supplementalLocation, context.CancellationToken))
				{
					context.RegisterCodeFix(
						CreateReplaceWithIfAction(c => FixRequires(document, diagnostic, supplementalLocation, "Invalid Argument.", c)),
						diagnostic);
				}
			}
		}

		static async Task<Document> FixByDelete(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			return document.WithSyntaxRoot(
				CodeContractsUsingSimplifier.Instance.Visit(
					root.RemoveNode(
						root.FindNode(diagnostic.Location.SourceSpan),
						SyntaxRemoveOptions.AddElasticMarker | SyntaxRemoveOptions.KeepExteriorTrivia)));
		}

		static async Task<Document> FixGenericRequires(Document document, Diagnostic diagnostic, Location typeLocation, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invoke = (InvocationExpressionSyntax)statementNode.Expression;

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var replacement = CodeContractsHelper.ConvertGenericRequires(semanticModel, invoke, typeLocation, cancellationToken);

			return document.WithSyntaxRoot(
				CodeContractsUsingSimplifier.Instance.Visit(
					root.ReplaceNode(statementNode, CodeContractsHelper.WithElasticTriviaFrom(replacement, statementNode))));
		}

		static async Task<Document> FixRequiresNotNull(Document document, Diagnostic diagnostic, Location identifierLocation, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var replacement = CodeContractsHelper.ConvertRequiresNotNull(invokeNode, identifierLocation);

			return document.WithSyntaxRoot(
				CodeContractsUsingSimplifier.Instance.Visit(
					root.ReplaceNode(statementNode, CodeContractsHelper.WithElasticTriviaFrom(replacement, statementNode))));
		}

		static async Task<Document> FixRequires(Document document, Diagnostic diagnostic, Location identifierLocation, string defaultMessage, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var replacement = CodeContractsHelper.ConvertRequires(invokeNode, identifierLocation, defaultMessage);

			return document.WithSyntaxRoot(
				CodeContractsUsingSimplifier.Instance.Visit(
					root.ReplaceNode(statementNode, CodeContractsHelper.WithElasticTriviaFrom(replacement, statementNode))));
		}

		static CodeAction CreateDeleteAction(Document document, Diagnostic diagnostic)
		{
			return CodeAction.Create(
				"Delete.",
				c => FixByDelete(document, diagnostic, c),
				equivalenceKey: "Delete");
		}

		static CodeAction CreateReplaceWithIfAction(Func<CancellationToken, Task<Document>> createChangedDocument)
		{
			return CodeAction.Create(
				"Replace with 'if' check.",
				createChangedDocument,
				equivalenceKey: "ReplaceWithIf");
		}
	}
}
