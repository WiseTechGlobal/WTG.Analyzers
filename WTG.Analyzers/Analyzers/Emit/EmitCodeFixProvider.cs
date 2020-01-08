using System.Collections.Immutable;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmitCodeFixProvider))]
	public sealed class EmitCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			Rules.UseCorrectEmitOverloadDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var document = context.Document;
			var diagnostic = context.Diagnostics.First();

			if (diagnostic.Properties.TryGetValue(EmitAnalyzer.SuggestedFixProperty, out var fixCode))
			{
				switch (fixCode)
				{
					case EmitAnalyzer.DeleteArgument:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Remove Argument",
								createChangedDocument: c => RemoveArgumentFixAsync(document, diagnostic, c),
								equivalenceKey: "RemoveArgument"),
							diagnostic);
						break;

					case EmitAnalyzer.ConvertArgument:
						context.RegisterCodeFix(
							CodeAction.Create(
								"Convert Argument",
								createChangedDocument: c => ConvertArgumentFixAsync(document, diagnostic, c),
								equivalenceKey: "ConvertArgument"),
							diagnostic);
						break;
				}
			}

			return Task.CompletedTask;
		}

		static async Task<Document> RemoveArgumentFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var invoke = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invoke,
					RemoveArgument(invoke)));
		}

		static async Task<Document> ConvertArgumentFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var invoke = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					invoke,
					ConvertArgument(invoke)));
		}

		static SyntaxNode RemoveArgument(InvocationExpressionSyntax invoke)
		{
			var argList = invoke.ArgumentList;

			if (invoke.Expression is MemberAccessExpressionSyntax expression &&
				expression.Name.Identifier.Text != EmitMatrix.Emit)
			{
				invoke = invoke.WithExpression(
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						expression.Expression,
						SyntaxFactory.IdentifierName(EmitMatrix.Emit)));
			}

			return invoke.WithArgumentList(
				argList.WithArguments(
					SyntaxFactory.SeparatedList(
						new[] { argList.Arguments[0] })));
		}

		static InvocationExpressionSyntax ConvertArgument(InvocationExpressionSyntax invoke)
		{
			var argList = invoke.ArgumentList;
			var field = argList.Arguments[0].Accept(FieldAccessor.Instance);

			if (field == null)
			{
				return invoke;
			}

			var opcode = EmitMatrix.GetOpCode(field.Identifier.Text);

			if (opcode == OpCode.Invalid)
			{
				return invoke;
			}

			var valueArgument = argList.Arguments[1].Expression;

			return invoke.ReplaceNode(
				valueArgument,
				valueArgument.Accept(new EmitConversionVisitor(opcode.GetOperand())));
		}
	}
}
