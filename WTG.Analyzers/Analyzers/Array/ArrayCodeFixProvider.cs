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
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArrayCodeFixProvider))]
	[Shared]
	public sealed class ArrayCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Rules.PreferArrayEmptyOverNewArrayConstructionDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.PreferArrayEmptyOverNewArrayConstructionDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Change to Array.Empty<T>()",
							createChangedDocument: c => ReplaceWithArrayEmpty(context.Document, diagnostic, c),
							equivalenceKey: "ChangeToArrayEmptyT"),
						diagnostic: diagnostic);
					break;
			}

			return Task.CompletedTask;
		}

		static async Task<Document> ReplaceWithArrayEmpty(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (node.Kind() == SyntaxKind.ArrayCreationExpression)
			{
				var syntax = (ArrayCreationExpressionSyntax)node;

				document = document.WithSyntaxRoot(
					root.ReplaceNode(
						node,
						CreateArrayEmptyInvocation(
							syntax.Type.ElementType,
							CanUseTypeShortName(model, node.GetLocation(), "System.Array", "Array"))));
			}

			return document;
		}

		static bool CanUseTypeShortName(SemanticModel model, Location location, string typeFullName, string shortName)
		{
			var symbols = model.LookupSymbols(location.SourceSpan.Start, null, shortName);
			if (symbols.Length == 0)
			{
				return false;
			}

			var symbol = symbols[0];
			if (symbol.Kind != SymbolKind.NamedType)
			{
				return false;
			}

			var namedSymbol = (INamedTypeSymbol)symbol;
			return namedSymbol.IsMatch(typeFullName);
		}

		static InvocationExpressionSyntax CreateArrayEmptyInvocation(TypeSyntax elementType, bool canUseShortName)
		{
			ExpressionSyntax arrayIdentifier;

			if (canUseShortName)
			{
				arrayIdentifier = SyntaxFactory.IdentifierName("Array");
			}
			else
			{
				arrayIdentifier = SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.IdentifierName("System"),
					SyntaxFactory.IdentifierName("Array"));
			}

			return SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					arrayIdentifier,
					SyntaxFactory.GenericName(
						SyntaxFactory.Identifier("Empty"))
					.WithTypeArgumentList(
						SyntaxFactory.TypeArgumentList(
							SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
								elementType)))));
		}
	}
}
