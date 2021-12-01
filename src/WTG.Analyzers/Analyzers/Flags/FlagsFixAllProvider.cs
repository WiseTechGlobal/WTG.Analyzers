using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class FlagsFixAllProvider : DocumentBatchedFixAllProvider
	{
		FlagsFixAllProvider()
		{
		}

		public static FixAllProvider Instance { get; } = new FlagsFixAllProvider();

		protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await originalDocument.RequireSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await originalDocument.RequireSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var group = new Dictionary<EnumDeclarationSyntax, List<EnumMemberDeclarationSyntax>>();

			foreach (var diagnostic in diagnostics)
			{
				var member = (EnumMemberDeclarationSyntax)root.FindNode(diagnostic.Location.SourceSpan);
				var decl = (EnumDeclarationSyntax?)member.Parent;
				NRT.Assert(decl != null, "The fixer should only be running on a full and complete document.");

				if (!group.TryGetValue(decl, out var list))
				{
					group.Add(decl, list = new List<EnumMemberDeclarationSyntax>());
				}

				list.Add(member);
			}

			return documentToFix.WithSyntaxRoot(
				root.ReplaceNodes(
					group.Keys,
					(originalDecl, targetDecl) =>
					{
						return SetValues(semanticModel, originalDecl, group[originalDecl]);
					}));
		}

		static SyntaxNode SetValues(SemanticModel semanticModel, EnumDeclarationSyntax decl, List<EnumMemberDeclarationSyntax> members)
		{
			var available = FlagsHelper.AvailableBitMask(semanticModel, decl);

			return decl.ReplaceNodes(
				members,
				(originalMember, targetMember) =>
				{
					if (FlagsHelper.IsNone(originalMember))
					{
						return originalMember.WithEqualsValue(
							SyntaxFactory.EqualsValueClause(
								ExpressionSyntaxFactory.CreateLiteral(0)));
					}
					else
					{
						var index = FlagsHelper.TakeNextAvailableIndex(ref available);

						return originalMember.WithEqualsValue(
							SyntaxFactory.EqualsValueClause(
								ExpressionSyntaxFactory.CreateSingleBitFlag(index)));
					}
				});
		}
	}
}
