using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class DocumentBatchedFixAllProviderTest
	{
		[TestCase(FixAllScope.Document, ExpectedResult = FixedDocument)]
		[TestCase(FixAllScope.Project, ExpectedResult = FixedProject)]
		[TestCase(FixAllScope.Solution, ExpectedResult = FixedSolution)]
		public async Task<string> BulkUpdate(FixAllScope scope)
		{
			var target = ModelUtils
				.CreateProject("using TargetDocument;", "using LocalDocument;")
				.AddAdHocDependency("Related", "using ForeignDocument;")
				.Documents
				.First();

			var context = CreateContext(target, scope);
			var codeAction = await new TestFixAllProvider().GetFixAsync(context).ConfigureAwait(false);
			var newDocument = await ApplyFixAsync(target, codeAction).ConfigureAwait(false);
			return await Format(newDocument.Project.Solution).ConfigureAwait(false);
		}

		#region Implementation

		static FixAllContext CreateContext(Document targetDocument, FixAllScope scope)
		{
			var result = new TestProvider();

			foreach (var document in targetDocument.Project.Solution.Projects.SelectMany(x => x.Documents))
			{
				result.Add(document, "Diagnostic A");
				result.Add(document, "Diagnostic B");
			}

			return new FixAllContext(
				targetDocument,
				new TestCodeFixProvider(),
				scope,
				"key",
				new[] { TestCodeFixProvider.ID },
				result,
				default);
		}

		static async Task<string> Format(Solution solution)
		{
			var builder = new StringBuilder();

			foreach (var project in solution.Projects)
			{
				builder.Append('[');
				builder.Append(project.Name);
				builder.Append(']');
				builder.AppendLine();

				foreach (var document in project.Documents)
				{
					builder.Append('<');
					builder.Append(document.Name);
					builder.Append('>');
					builder.AppendLine();

					var source = await document.GetTextAsync().ConfigureAwait(false);
					builder.AppendLine(source.ToString());
				}
			}

			return builder.ToString();
		}

		static async Task<Document> ApplyFixAsync(Document document, CodeAction codeAction)
		{
			var operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
			var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			return solution.GetDocument(document.Id);
		}

		sealed class TestFixAllProvider : DocumentBatchedFixAllProvider
		{
			protected override async Task<Document> ApplyFixesAsync(Document originalDocument, Document documentToFix, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
			{
				var source = await documentToFix.GetTextAsync().ConfigureAwait(false);
				var builder = new StringBuilder(source.ToString());
				builder.AppendLine();
				builder.Append("// ---");

				foreach (var diagnostic in diagnostics)
				{
					builder.AppendLine();
					builder.Append("// ");
					builder.Append(diagnostic.GetMessage());
				}

				return documentToFix.WithText(SourceText.From(builder.ToString()));
			}
		}

		sealed class TestCodeFixProvider : CodeFixProvider
		{
			public const string ID = "TestDiagnosticID";

			public override ImmutableArray<string> FixableDiagnosticIds => throw new NotImplementedException();
			public override Task RegisterCodeFixesAsync(CodeFixContext context) => throw new NotImplementedException();
		}

		sealed class TestProvider : FixAllContext.DiagnosticProvider
		{
			public Diagnostic Add(Document document, string message)
			{
				var location = document.GetSyntaxRootAsync().Result.GetLocation();

				var diagnostic = Diagnostic.Create(
					TestCodeFixProvider.ID,
					"category",
					message,
					DiagnosticSeverity.Error,
					DiagnosticSeverity.Error,
					true,
					0,
					false,
					location: location);

				Add(document, diagnostic);
				return diagnostic;
			}

			public void Add(Document document, Diagnostic diagnostic) => list.Add(new KeyValuePair<Document, Diagnostic>(document, diagnostic));

			public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
				=> Task.FromResult(list.Where(x => x.Key.Project == project).Select(x => x.Value).ToArray().AsEnumerable());

			public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
				=> Task.FromResult(list.Where(x => x.Key == document).Select(x => x.Value).ToArray().AsEnumerable());

			public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			readonly List<KeyValuePair<Document, Diagnostic>> list = new List<KeyValuePair<Document, Diagnostic>>();
		}

		const string FixedDocument =
@"[TestProject]
<Test0.cs>
using TargetDocument;
// ---
// Diagnostic A
// Diagnostic B
<Test1.cs>
using LocalDocument;
[Related]
<Test0.cs>
using ForeignDocument;
";

		const string FixedProject =
@"[TestProject]
<Test0.cs>
using TargetDocument;
// ---
// Diagnostic A
// Diagnostic B
<Test1.cs>
using LocalDocument;
// ---
// Diagnostic A
// Diagnostic B
[Related]
<Test0.cs>
using ForeignDocument;
";

		const string FixedSolution =
@"[TestProject]
<Test0.cs>
using TargetDocument;
// ---
// Diagnostic A
// Diagnostic B
<Test1.cs>
using LocalDocument;
// ---
// Diagnostic A
// Diagnostic B
[Related]
<Test0.cs>
using ForeignDocument;
// ---
// Diagnostic A
// Diagnostic B
";
		#endregion
	}
}
