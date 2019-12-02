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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToStringCodeFixProvider))]
	[Shared]
	public sealed class ToStringCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontCallToStringOnAStringDiagnosticID,
			Rules.PreferNameofOverCallingToStringOnAnEnumDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			switch (diagnostic.Id)
			{
				case Rules.DontCallToStringOnAStringDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Remove ToString()",
							createChangedDocument: c => RemoveToString(context.Document, diagnostic, c),
							equivalenceKey: "RemoveToString"),
						diagnostic: diagnostic);
					break;

				case Rules.PreferNameofOverCallingToStringOnAnEnumDiagnosticID:
					context.RegisterCodeFix(
						CodeAction.Create(
							title: "Convert to nameof.",
							createChangedDocument: c => ConvertToNameof(context.Document, diagnostic, c),
							equivalenceKey: "ConvertToNameof"),
						diagnostic: diagnostic);
					break;
			}

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveToString(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var invoke = GetTargetInvoke(root, diagnostic);

			switch (invoke.Expression.Kind())
			{
				case SyntaxKind.SimpleMemberAccessExpression:
					// value.ToString() --> value
					return document.WithSyntaxRoot(
						root.ReplaceNode(
							invoke,
							Unwrap(invoke)
							.WithTriviaFrom(invoke)));

				case SyntaxKind.MemberBindingExpression:
					switch (invoke.Parent.Kind())
					{
						case SyntaxKind.ConditionalAccessExpression:
							var conditionalAccess = (ConditionalAccessExpressionSyntax)invoke.Parent;

							if (conditionalAccess.WhenNotNull == invoke)
							{
								// value?.ToString() --> value
								return document.WithSyntaxRoot(
									root.ReplaceNode(
										conditionalAccess,
										conditionalAccess.Expression
											.WithTriviaFrom(conditionalAccess)));
							}
							else
							{
								// value?.ToString()?.Length --> value?.Length
								return document.WithSyntaxRoot(
									root.ReplaceNode(
										conditionalAccess,
										conditionalAccess.WhenNotNull
											.WithTriviaFrom(conditionalAccess)));
							}

						case SyntaxKind.SimpleMemberAccessExpression:
							{
								// value?.ToString().Length --> value?.Length
								var member = (MemberAccessExpressionSyntax)invoke.Parent;
								var binding = (MemberBindingExpressionSyntax)invoke.Expression;

								return document.WithSyntaxRoot(
									root.ReplaceNode(
										member,
										binding.WithName(member.Name)
											.WithTriviaFrom(member)));
							}

						case SyntaxKind.ElementAccessExpression:
							{
								// value?.ToString()[0] --> value?[0]
								var member = (ElementAccessExpressionSyntax)invoke.Parent;

								return document.WithSyntaxRoot(
									root.ReplaceNode(
										member,
										SyntaxFactory.ElementBindingExpression(member.ArgumentList)
											.WithTriviaFrom(member)));
							}
					}
					break;
			}

			return document;
		}

		static async Task<Document> ConvertToNameof(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var invoke = GetTargetInvoke(root, diagnostic);

			var nameofExpression = ExpressionSyntaxFactory.CreateNameof(Unwrap(invoke).WithoutTrivia())
				.WithTriviaFrom(invoke);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invoke,
					nameofExpression));
		}

		static InvocationExpressionSyntax GetTargetInvoke(SyntaxNode root, Diagnostic diagnostic)
		{
			return (InvocationExpressionSyntax)root.FindNode(
				diagnostic.Location.SourceSpan,
				getInnermostNodeForTie: true);
		}

		static ExpressionSyntax Unwrap(InvocationExpressionSyntax expression)
		{
			var member = (MemberAccessExpressionSyntax)expression.Expression;
			var tmp = member.Expression;

			while (tmp.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				var exp = (ParenthesizedExpressionSyntax)tmp;

				tmp = exp.Expression;
			}

			return tmp;
		}
	}
}
