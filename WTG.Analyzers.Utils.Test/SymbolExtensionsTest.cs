using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class SymbolExtensionsTest
	{
		[Test]
		public void IsValueTuple()
		{
			Assert.That(SingleMatchIndex(types, x => x.IsValueTuple()), Is.EqualTo(4));
		}

		[TestCase("mscorlib", ExpectedResult = 0)]
		[TestCase("System.Core", ExpectedResult = 1)]
		public int MatchAssembly(string assemblyName)
		{
			return SingleMatchIndex(assemblies, x => x.IsMatch(assemblyName));
		}

		[TestCase("System.Threading.Tasks.Task", ExpectedResult = 0)]
		[TestCase("System.Linq.Enumerable", ExpectedResult = 1)]
		[TestCase("System.Collections.Generic.List`1", ExpectedResult = 2)]
		[TestCase("System.Collections.Generic.List`1+Enumerator", ExpectedResult = 3)]
		public int MatchType(string typeName)
		{
			return SingleMatchIndex(types, x => x.IsMatch(typeName));
		}

		[TestCase("mscorlib", "System.Threading.Tasks.Task", ExpectedResult = 0)]
		[TestCase("System.Core", "System.Linq.Enumerable", ExpectedResult = 1)]
		[TestCase("mscorlib", "System.Collections.Generic.List`1", ExpectedResult = 2)]
		[TestCase("mscorlib", "System.Collections.Generic.List`1+Enumerator", ExpectedResult = 3)]
		public int MatchType(string assemblyName, string typeName)
		{
			return SingleMatchIndex(types, x => x.IsMatch(assemblyName, typeName));
		}

		[TestCase("System.Threading.Tasks.Task", "FromResult", ExpectedResult = 0)]
		[TestCase("System.Threading.Tasks.Task", "FromException", ExpectedResult = 1)]
		public int MatchMethod(string typeName, string methodName)
		{
			return SingleMatchIndex(methods, x => x.IsMatch(typeName, methodName));
		}

		[TestCase("mscorlib", "System.Threading.Tasks.Task", "FromResult", ExpectedResult = 0)]
		[TestCase("mscorlib", "System.Threading.Tasks.Task", "FromException", ExpectedResult = 1)]
		public int MatchMethod(string assemblyName, string typeName, string methodName)
		{
			return SingleMatchIndex(methods, x => x.IsMatch(assemblyName, typeName, methodName));
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
			return items.Select((x, i) => new { x, i }).Single(x => predicate(x.x)).i;
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
