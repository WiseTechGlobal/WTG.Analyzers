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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeContractsCodeFixProvider))]
	[Shared]
	public sealed class CodeContractsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DoNotUseCodeContractsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			if (diagnostic.Properties.TryGetValue(CodeContractsAnalyzer.PropertyProposedFix, out var proposedFix))
			{
				switch (proposedFix)
				{
					case CodeContractsAnalyzer.FixDelete:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Remove Contract Usage.",
								c => FixByDelete(context.Document, diagnostic, c),
								equivalenceKey: "FIX"),
							diagnostic);
						break;

					case CodeContractsAnalyzer.FixGenericRequires:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Replace with 'if' check.",
								c => FixGenericRequires(context.Document, diagnostic, c),
								equivalenceKey: "FIX"),
							diagnostic);
						break;

					case CodeContractsAnalyzer.FixRequiresNonNull:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Replace with 'if' check.",
								c => FixRequiresNotNull(context.Document, diagnostic, c),
								equivalenceKey: "FIX"),
							diagnostic);
						break;
				}
			}

			return Task.FromResult<object>(null);
		}

		static async Task<Document> FixByDelete(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.RemoveNode(node, SyntaxRemoveOptions.AddElasticMarker | SyntaxRemoveOptions.KeepExteriorTrivia));
		}

		static async Task<Document> FixGenericRequires(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var exceptionType = GetGenericTypeArgument(invokeNode);
			var arguments = invokeNode.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.LogicalNot(arguments[0].Expression);

			var replacement = CreateGuardClause(
				condition,
				exceptionType,
				arguments.Count > 1 ? SyntaxFactory.ArgumentList(arguments.RemoveAt(0)) : SyntaxFactory.ArgumentList());

			return document.WithSyntaxRoot(
				root.ReplaceNode(statementNode, replacement));

			TypeSyntax GetGenericTypeArgument(InvocationExpressionSyntax node)
			{
				var access = (MemberAccessExpressionSyntax)node.Expression;
				var name = (GenericNameSyntax)access.Name;
				var typeArgs = name.TypeArgumentList.Arguments;
				return typeArgs.Count > 0 ? typeArgs[0] : null;
			}
		}

		static async Task<Document> FixRequiresNotNull(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var statementNode = (ExpressionStatementSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var invokeNode = (InvocationExpressionSyntax)statementNode.Expression;
			var arguments = invokeNode.ArgumentList.Arguments;
			var condition = ExpressionSyntaxFactory.LogicalNot(arguments[0].Expression);

			ArgumentListSyntax argumentList;

			if (arguments.Count > 1)
			{
				// If an explicit message was provided to Requires(), then keep it.
				argumentList = SyntaxFactory.ArgumentList(arguments.RemoveAt(0));
			}
			else
			{
				var paramSyntax = (ExpressionSyntax)invokeNode.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);

				if (paramSyntax.IsKind(SyntaxKind.IdentifierName))
				{
					// If we can work out the name of the parameter, create a suitable 'nameof' expression.
					argumentList = SyntaxFactory.ArgumentList(
						SyntaxFactory.SeparatedList(new[]
						{
							SyntaxFactory.Argument(
								ExpressionSyntaxFactory.CreateNameof(paramSyntax))
						}));
				}
				else
				{
					// If all else fails, use the default constructor and let the developer sort it out.
					argumentList = SyntaxFactory.ArgumentList();
				}
			}

			var replacement = CreateGuardClause(
				condition,
				ArgumentNullExceptionTypeName,
				argumentList);

			return document.WithSyntaxRoot(
				root.ReplaceNode(statementNode, replacement));
		}

		static IfStatementSyntax CreateGuardClause(ExpressionSyntax condition, TypeSyntax exceptionType, ArgumentListSyntax argumentList)
		{
			return SyntaxFactory.IfStatement(
				condition,
				SyntaxFactory.Block(
					SyntaxFactory.ThrowStatement(
						SyntaxFactory.ObjectCreationExpression(
							exceptionType,
							argumentList,
							null))))
				.WithAdditionalAnnotations(Formatter.Annotation);
		}

		static readonly TypeSyntax ArgumentNullExceptionTypeName =
			SyntaxFactory.QualifiedName(
				SyntaxFactory.IdentifierName(nameof(System)),
				SyntaxFactory.IdentifierName(nameof(ArgumentNullException)))
			.WithAdditionalAnnotations(Simplifier.Annotation);
	}
}
