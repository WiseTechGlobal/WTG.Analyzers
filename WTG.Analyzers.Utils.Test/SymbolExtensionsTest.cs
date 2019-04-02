using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class SymbolExtensionsTest
	{
		[Test]
		public void IsValueTuple()
		{
			Assert.That(SingleMatchIndex(types, x => x.IsValueTuple()), Is.EqualTo(4));
		}

		[TestCase("System.Threading.Tasks.Task", ExpectedResult = 0)]
		[TestCase("System.Linq.Enumerable", ExpectedResult = 1)]
		[TestCase("System.Collections.Generic.List`1", ExpectedResult = 2)]
		[TestCase("System.Collections.Generic.List`1+Enumerator", ExpectedResult = 3)]
		[TestCase("System.ValueTuple`2", ExpectedResult = 4)]
		[TestCase("System.ValueTuple", ExpectedResult = -1)]
		[TestCase("System.Collections.Generic.List`1+Enum", ExpectedResult = -1)]
		[TestCase("Syste.Linq.Enumerable", ExpectedResult = -1)]
		public int MatchType(string typeName)
		{
			return SingleMatchIndex(types, x => x.IsMatch(typeName));
		}

		[TestCase("System.Threading.Tasks.Task", ExpectedResult = 0)]
		[TestCase("System.Linq.Enumerable", ExpectedResult = 1)]
		[TestCase("System.Collections.Generic.List", ExpectedResult = 2)]
		[TestCase("System.Collections.Generic.List+Enumerator", ExpectedResult = 3)]
		[TestCase("System.ValueTuple", ExpectedResult = 4)]
		[TestCase("System.Value", ExpectedResult = -1)]
		[TestCase("System.Collections.Generic.List+Enum", ExpectedResult = -1)]
		[TestCase("Syste.Linq.Enumerable", ExpectedResult = -1)]
		public int MatchAnyArity(string typeName)
		{
			return SingleMatchIndex(types, x => x.IsMatchAnyArity(typeName));
		}

		[TestCase("System.Threading.Tasks.Task", "FromResult", ExpectedResult = 0)]
		[TestCase("System.Threading.Tasks.Task", "FromException", ExpectedResult = 1)]
		public int MatchMethod(string typeName, string methodName)
		{
			return SingleMatchIndex(methods, x => x.IsMatch(typeName, methodName));
		}

		[TestCase("public class Foo { public void Method(); }", ExpectedResult = true)]
		[TestCase("public class Foo { public class Bar { public void Method(); } }", ExpectedResult = true)]
		[TestCase("public class Foo { public class Bar { void Method(); } }", ExpectedResult = false)]
		[TestCase("public class Foo { class Bar { public void Method(); } }", ExpectedResult = false)]
		[TestCase("class Foo { public class Bar { public void Method(); } }", ExpectedResult = false)]
		[TestCase("public class Foo { internal void Method(); }", ExpectedResult = false)]
		[TestCase("public class Foo { protected void Method(); }", ExpectedResult = true)]
		[TestCase("public class Foo { private protected void Method(); }", ExpectedResult = false)]
		[TestCase("public class Foo { protected internal void Method(); }", ExpectedResult = true)]
		[TestCase("public class Foo { private void Method(); }", ExpectedResult = false)]
		public async Task<bool> IsExternallyVisible(string source)
		{
			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			var model = compilation.GetSemanticModel(tree);

			var method = (MethodDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x.IsKind(SyntaxKind.MethodDeclaration));
			var symbol = model.GetDeclaredSymbol(method);

			return symbol.IsExternallyVisible();
		}

		[TestCase("public class Foo { public int Method() => 0; }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo { public int Method() => 0; } public interface IFoo { int Method(); }", ExpectedResult = true)]
		[TestCase("public class Foo : IFoo { public int Method() => 0; int IFoo.Method() => Method(); } public interface IFoo { void Method(); }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo { public int Method(object o) => 0; public int Method() => 0; } public interface IFoo { void Method(); }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo<int> { public string Method() => null; public int Method() => 0; } public interface IFoo<T> { T Method(); }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo<string> { public string Method() => null; public int Method() => 0; } public interface IFoo<T> { T Method(); }", ExpectedResult = true)]
		[TestCase("public class Foo { public int Property => 0; }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo { public int Property => 0; } public interface IFoo { int Property { get; } }", ExpectedResult = true)]
		[TestCase("public class Foo : IFoo { public int Property => 0; int IFoo.Property => Property; } public interface IFoo { int Property { get; } }", ExpectedResult = false)]
		[TestCase("public class Foo { public event System.Action Event; }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo { public event System.Action Event; } public interface IFoo { event System.Action Event; }", ExpectedResult = true)]
		[TestCase("public class Foo { public event System.Action Event { get { } set { } } }", ExpectedResult = false)]
		[TestCase("public class Foo : IFoo { public event System.Action Event { get { } set { } } } public interface IFoo { event System.Action Event; }", ExpectedResult = true)]
		public async Task<bool> ImplementsAnInterface(string source)
		{
			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			var model = compilation.GetSemanticModel(tree);

			var typeDecl = (ClassDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x.IsKind(SyntaxKind.ClassDeclaration));
			var syntax = (SyntaxNode)typeDecl.Members.First();

			if (syntax is EventFieldDeclarationSyntax e)
			{
				syntax = e.Declaration.Variables[0];
			}

			var symbol = model.GetDeclaredSymbol(syntax);

			return symbol.ImplementsAnInterface();
		}

		#region Implementation

		[OneTimeSetUp]
		protected async Task Setup()
		{
			var source =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
";

			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			semanticModel = compilation.GetSemanticModel(tree);
			pos = tree.Length;

			types = new[]
			{
				GetType("Task"),
				GetType("Enumerable"),
				GetType("List<int>"),
				GetType("List<int>.Enumerator"),
				GetType("ValueTuple<string, int>"),
			};

			assemblies = new[]
			{
				types[0].ContainingAssembly,
				types[1].ContainingAssembly,
			};

			methods = new[]
			{
				(IMethodSymbol)GetExpressionSymbol("Task.FromResult(42)"),
				(IMethodSymbol)GetExpressionSymbol("Task.FromException(default(Exception))"),
			};
		}

		static int SingleMatchIndex<T>(T[] items, Predicate<T> predicate)
		{
			for (var i = 0; i < items.Length; i++)
			{
				if (predicate(items[i]))
				{
					return i;
				}
			}

			return -1;
		}

		ITypeSymbol GetType(string source)
		{
			var syntax = SyntaxFactory.ParseTypeName(source);

			if (syntax == null)
			{
				throw new ArgumentException("Parse fail: " + source);
			}

			var info = semanticModel.GetSpeculativeSymbolInfo(pos, syntax, SpeculativeBindingOption.BindAsTypeOrNamespace);

			if (info.Symbol == null)
			{
				throw new ArgumentException("Semantic fail: " + info.CandidateReason + ": " + source);
			}

			return (ITypeSymbol)info.Symbol;
		}

		ISymbol GetExpressionSymbol(string source)
		{
			var syntax = SyntaxFactory.ParseExpression(source);

			if (syntax == null)
			{
				throw new ArgumentException("Parse fail: " + source);
			}

			var info = semanticModel.GetSpeculativeSymbolInfo(pos, syntax, SpeculativeBindingOption.BindAsExpression);

			if (info.Symbol == null)
			{
				throw new ArgumentException("Semantic fail: " + info.CandidateReason + ": " + source);
			}

			return info.Symbol;
		}

		SemanticModel semanticModel;
		int pos;
		IAssemblySymbol[] assemblies;
		ITypeSymbol[] types;
		IMethodSymbol[] methods;

		#endregion
	}
}
