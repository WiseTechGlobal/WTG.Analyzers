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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FlagsCodeFixProvider))]
	[Shared]
	public sealed class FlagsCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			Rules.FlagEnumsShouldSpecifyExplicitValuesDiagnosticID);

		public override FixAllProvider GetFixAllProvider() => FlagsFixAllProvider.Instance;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Set an explicit value.",
					createChangedDocument: c => SetEnumMemberValue(context.Document, diagnostic, c),
					equivalenceKey: "SetExplicitValue"),
				diagnostic);

			return Task.CompletedTask;
		}

		static async Task<Document> SetEnumMemberValue(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var root = await document.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var model = await document.RequireSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var member = (EnumMemberDeclarationSyntax)root.FindNode(diagnostic.Location.SourceSpan);
			var decl = (EnumDeclarationSyntax?)member.Parent;
			NRT.Assert(decl != null, "The fixer should only be running on a full and complete document.");

			ExpressionSyntax explicitValue;

			if (FlagsHelper.IsNone(member))
			{
				explicitValue = ExpressionSyntaxFactory.CreateLiteral(0);
			}
			else
			{
				var available = FlagsHelper.AvailableBitMask(model, decl);
				var index = FlagsHelper.TakeNextAvailableIndex(ref available);
				explicitValue = ExpressionSyntaxFactory.CreateSingleBitFlag(index);
			}

			return document.WithSyntaxRoot(
				root.ReplaceNode(
					member,
					member.WithEqualsValue(
						SyntaxFactory.EqualsValueClause(
							explicitValue))));
		}
	}
}
