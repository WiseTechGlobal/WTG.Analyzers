using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringBuilderAppendCodeFixProvider))]
	[Shared]
	public sealed class StringBuilderAppendCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.DontMutateAppendedStringArgumentsDiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var document = context.Document;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Convert to use appends.",
					createChangedDocument: c => ConvertToAppendsAsync(diagnostic, document, c),
					equivalenceKey: "ConvertToAppends"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> ConvertToAppendsAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticSpan = diagnostic.AdditionalLocations[0].SourceSpan;
			var invocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
			var memberExpression = (MemberAccessExpressionSyntax)invocation.Expression;
			var appendLine = memberExpression.Name.Identifier.Text == nameof(StringBuilder.AppendLine);
			var firstArgument = invocation.ArgumentList.Arguments[0].Expression;

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invocation,
					Translate(
						await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false),
						memberExpression.Expression.WithoutTrailingTrivia(),
						firstArgument,
						appendLine,
						cancellationToken)
						.WithTrailingTrivia(invocation.GetTrailingTrivia())));
		}

		static ExpressionSyntax Translate(SemanticModel semanticModel, ExpressionSyntax baseExpression, ExpressionSyntax valueExpression, bool appendLine, CancellationToken cancellationToken)
		{
			if (valueExpression.IsKind(SyntaxKind.AddExpression))
			{
				var binaryExpression = (BinaryExpressionSyntax)valueExpression;

				return Translate(
					semanticModel,
					Translate(
						semanticModel,
						baseExpression,
						binaryExpression.Left,
						appendLine: false,
						cancellationToken),
					binaryExpression.Right,
					appendLine,
					cancellationToken);
			}

			switch (GetCategory(semanticModel, valueExpression, cancellationToken))
			{
				case Category.Format:
					baseExpression = Invoke(baseExpression, AppendFormat, GetFormatArguments(semanticModel, valueExpression, cancellationToken));
					break;

				case Category.StringValue:
					return Invoke(baseExpression, appendLine ? AppendLine : Append, valueExpression);

				case Category.Substring:
					baseExpression = Invoke(baseExpression, Append, GetSubstringArguments((InvocationExpressionSyntax)valueExpression));
					break;

				default:
					baseExpression = Invoke(baseExpression, Append, valueExpression);
					break;
			}

			if (appendLine)
			{
				baseExpression = Invoke(baseExpression, AppendLine);
			}

			return baseExpression;
		}

		static Category GetCategory(SemanticModel semanticModel, ExpressionSyntax valueExpression, CancellationToken cancellationToken)
		{
			if (valueExpression.IsKind(SyntaxKind.InterpolatedStringExpression))
			{
				return Category.Format;
			}
			else if (valueExpression.IsKind(SyntaxKind.InvocationExpression))
			{
				var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(valueExpression, cancellationToken).Symbol;

				if (methodSymbol != null && methodSymbol.ContainingType.SpecialType == SpecialType.System_String)
				{
					if (methodSymbol.Name == nameof(string.Format))
					{
						return Category.Format;
					}
					else if (methodSymbol.Name == nameof(string.Substring))
					{
						return Category.Substring;
					}
				}

				return Category.StringValue;
			}

			var type = semanticModel.GetTypeInfo(valueExpression, cancellationToken).Type;

			return type != null && type.SpecialType == SpecialType.System_String
				? Category.StringValue
				: Category.NonStringValue;
		}

		static ArgumentListSyntax GetFormatArguments(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			if (expression.IsKind(SyntaxKind.InvocationExpression))
			{
				return ((InvocationExpressionSyntax)expression).ArgumentList;
			}

			var info = InterpolationInfo.Extract(semanticModel, (InterpolatedStringExpressionSyntax)expression, cancellationToken);
			var arguments = new List<ArgumentSyntax>();
			arguments.Add(SyntaxFactory.Argument(ExpressionSyntaxFactory.CreateLiteral(info.Format)));
			arguments.AddRange(info.Expressions.Select(SyntaxFactory.Argument));
			return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
		}

		static ArgumentListSyntax GetSubstringArguments(InvocationExpressionSyntax expression)
		{
			var member = (MemberAccessExpressionSyntax)expression.Expression;

			var oldArguments = expression.ArgumentList.Arguments;
			var newArguments = new ArgumentSyntax[3];
			newArguments[0] = SyntaxFactory.Argument(member.Expression);
			newArguments[1] = oldArguments[0];

			if (oldArguments.Count == 2)
			{
				newArguments[2] = oldArguments[1];
			}
			else
			{
				// This should work fine when the provided arguments are simple, but will produce 'sub-optimal' results
				// if the arguments are complex or have side-effects. Hopefully we can rely on the developer's better
				// judgement in these cases.
				newArguments[2] = SyntaxFactory.Argument(
					SyntaxFactory.BinaryExpression(
						SyntaxKind.SubtractExpression,
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							newArguments[0].Expression,
							SyntaxFactory.IdentifierName(nameof(string.Length))),
						newArguments[1].Expression));
			}

			return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(newArguments));
		}

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method)
			=> Invoke(baseExpression, method, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>()));

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method, ExpressionSyntax argument)
			=> Invoke(baseExpression, method, SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument))));

		static ExpressionSyntax Invoke(ExpressionSyntax baseExpression, IdentifierNameSyntax method, ArgumentListSyntax arguments)
		{
			return SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					baseExpression,
					method),
				arguments);
		}

		static readonly IdentifierNameSyntax Append = SyntaxFactory.IdentifierName(nameof(StringBuilder.Append));
		static readonly IdentifierNameSyntax AppendLine = SyntaxFactory.IdentifierName(nameof(StringBuilder.AppendLine));
		static readonly IdentifierNameSyntax AppendFormat = SyntaxFactory.IdentifierName(nameof(StringBuilder.AppendFormat));

		enum Category
		{
			Format,
			Substring,
			StringValue,
			NonStringValue,
		}
	}
}
