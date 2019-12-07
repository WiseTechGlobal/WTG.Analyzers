using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	abstract class LinqMethod
	{
		public static LinqMethod Any { get; } = new AnyLinqMethod();
		public static LinqMethod Count { get; } = new CountLinqMethod();
		public static LinqMethod ElementAt { get; } = new ElementAtLinqMethod();
		public static LinqMethod First { get; } = new FirstLinqMethod();

		public static LinqMethod Find(string methodName)
		{
			return methodName switch
			{
				nameof(Enumerable.Any) => Any,
				nameof(Enumerable.Count) => Count,
				nameof(Enumerable.ElementAt) => ElementAt,
				nameof(Enumerable.First) => First,
				_ => null,
			};
		}

		protected LinqMethod()
		{
		}

		public abstract bool IsMatch(IMethodSymbol method);
		public abstract LinqResolution GetResolution(ITypeSymbol sourceType);

		protected static bool IsEnumerableLinqMethod(IMethodSymbol method, string methodName, int paramCount)
		{
			var exm = method.ReducedFrom ?? method;

			return exm.IsMatch("System.Linq.Enumerable", methodName)
				&& exm.Parameters.Length == paramCount;
		}

		protected static bool HasCountProperty(ITypeSymbol type) => HasProperty(type, "Count");
		protected static bool HasLengthProperty(ITypeSymbol type) => HasProperty(type, "Length");
		protected static bool HasIsEmptyProperty(ITypeSymbol type) => HasProperty(type, "IsEmpty");

		protected static bool HasIndexer(ITypeSymbol type)
		{
			foreach (var tmp in TypesInMemberSearchOrder(type))
			{
				if (DefinesInstanceIndexer(tmp))
				{
					return true;
				}
			}

			return false;
		}

		protected static Diagnostic CreatePropertyDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, string targetName, string propertyName, bool isInAnExpression)
		{
			if (isInAnExpression)
			{
				return Rules.CreatePreferDirectMemberAccessOverLinqInAnExpression_UsePropertyDiagnostic(
					GetInvokeLocation(invoke),
					targetName,
					sourceType.ToString(),
					propertyName);
			}
			else
			{
				return Rules.CreatePreferDirectMemberAccessOverLinq_UsePropertyDiagnostic(
					GetInvokeLocation(invoke),
					targetName,
					sourceType.ToString(),
					propertyName);
			}
		}

		protected static Diagnostic CreateIndexerDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, string targetName, bool isInAnExpression)
		{
			if (isInAnExpression)
			{
				return Rules.CreatePreferDirectMemberAccessOverLinqInAnExpression_UseIndexerDiagnostic(
					GetInvokeLocation(invoke),
					targetName,
					sourceType.ToString());
			}
			else
			{
				return Rules.CreatePreferDirectMemberAccessOverLinq_UseIndexerDiagnostic(
					GetInvokeLocation(invoke),
					targetName,
					sourceType.ToString());
			}
		}

		/// <summary>
		/// Generate a location covering the method name and any arguments, but not the instance expression.
		/// </summary>
		static Location GetInvokeLocation(InvocationExpressionSyntax invoke)
		{
			var name = ExpressionHelper.GetMethodName(invoke);
			var argSpan = invoke.ArgumentList.Span;

			return Location.Create(
				invoke.SyntaxTree,
				TextSpan.FromBounds(
					name.SpanStart,
					argSpan.End));
		}

		static bool HasProperty(ITypeSymbol type, string name)
		{
			foreach (var tmp in TypesInMemberSearchOrder(type))
			{
				if (DefinesInstanceProperty(tmp, name))
				{
					return true;
				}
			}

			return false;
		}

		static IEnumerable<ITypeSymbol> TypesInMemberSearchOrder(ITypeSymbol type)
		{
			if (type.TypeKind == TypeKind.Interface)
			{
				yield return type;

				foreach (var iface in type.AllInterfaces)
				{
					yield return iface;
				}
			}
			else
			{
				for (; type != null; type = type.BaseType)
				{
					yield return type;
				}
			}
		}

		static bool DefinesInstanceProperty(ITypeSymbol type, string name)
		{
			foreach (var member in type.GetMembers(name))
			{
				if (!member.IsStatic &&
					member.DeclaredAccessibility == Accessibility.Public &&
					member.Kind == SymbolKind.Property)
				{
					return true;
				}
			}

			return false;
		}

		static bool DefinesInstanceIndexer(ITypeSymbol type)
		{
			foreach (var member in type.GetMembers())
			{
				if (member.Kind == SymbolKind.Property)
				{
					var property = (IPropertySymbol)member;

					if (!property.IsStatic &&
						property.DeclaredAccessibility == Accessibility.Public &&
						property.IsIndexer &&
						property.Parameters.Length == 1 &&
						property.Parameters[0].Type.SpecialType == SpecialType.System_Int32)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
