using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Framework.Test
{
	[TestFixture]
	public class CodeFixerTest
	{
		[Test]
		public async Task FixDocumentAsync_FixNotAlwaysProvided()
		{
			var analyzerMock = new AnalyzerMock();
			var fixProviderMock = new FixProviderMock();

			var fixer = new CodeFixer(analyzerMock, fixProviderMock);
			await fixer.VerifyFixAsync(
				ModelUtils.CreateDocument(
				@"using System;
				namespace MyNamespace
				{
					public class MyClass1 { }
					public class MyClass2 { }
					public class MyClass3 { }
				}"),
				@"using System;
				namespace MyNamespace
				{
					public class MyClass2 { }
				}")
				.ConfigureAwait(false);
		}

		[DiagnosticAnalyzer(LanguageNames.CSharp)]
		sealed class AnalyzerMock : DiagnosticAnalyzer
		{
			public static readonly DiagnosticDescriptor Error = new DiagnosticDescriptor("Error", string.Empty, string.Empty, string.Empty, DiagnosticSeverity.Error, true);

			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Error);

			public override void Initialize(AnalysisContext context)
			{
				context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
			}

			static void Analyze(SyntaxNodeAnalysisContext context)
			{
				context.ReportDiagnostic(Diagnostic.Create(Error, context.Node.GetLocation()));
			}
		}

		sealed class FixProviderMock : CodeFixProvider
		{
			public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AnalyzerMock.Error.Id);

			public override FixAllProvider GetFixAllProvider() => null;

			public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
			{
				var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
				var diagnostic = context.Diagnostics.First();
				var diagnosticSpan = diagnostic.Location.SourceSpan;

				var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
				if (declaration.Identifier.ToString() != "MyClass2")
				{
					context.RegisterCodeFix(
						CodeAction.Create(
							title: string.Empty,
							createChangedDocument: c => Delete(context.Document, declaration, c),
							equivalenceKey: string.Empty),
						diagnostic);
				}
			}

			static async Task<Document> Delete(Document document, ClassDeclarationSyntax localDeclaration, CancellationToken cancellationToken)
			{
				var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
				var newRoot = oldRoot.RemoveNode(localDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
				return document.WithSyntaxRoot(newRoot);
			}
		}
	}
}
