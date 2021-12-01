using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class NullComparisonAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontEquateValueTypesWithNullRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.EqualsExpression,
				SyntaxKind.NotEqualsExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var expression = (BinaryExpressionSyntax)context.Node;

			int indexOfNull;

			if (NullLiteralVisitor.Instance.Visit(expression.Left) && IsValueType(context.SemanticModel, expression.Right, context.CancellationToken))
			{
				indexOfNull = 0;
			}
			else if (NullLiteralVisitor.Instance.Visit(expression.Right) && IsValueType(context.SemanticModel, expression.Left, context.CancellationToken))
			{
				indexOfNull = 1;
			}
			else
			{
				return;
			}

			var binaryExpressionOperator = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken);
			if (binaryExpressionOperator.Symbol?.Kind == SymbolKind.Method)
			{
				var binaryOperatorMethod = (IMethodSymbol)binaryExpressionOperator.Symbol;
				if (binaryOperatorMethod.MethodKind == MethodKind.UserDefinedOperator && binaryOperatorMethod.Parameters.Length == 2)
				{
					var parameterForNull = binaryOperatorMethod.Parameters[indexOfNull];
					if (!IsValueType(parameterForNull.Type))
					{
						// User-defined conversion operator accepts reference types, so there is no compile-time guaranteed result from this comparison
						// to null that the compiler is optimizing away.
						return;
					}
				}
			}

			var leftConversion = context.SemanticModel.GetConversion(expression.Left, context.CancellationToken);
			var rightConversion = context.SemanticModel.GetConversion(expression.Right, context.CancellationToken);

			if (IsUserDefinedValueTypeConversion(leftConversion, rightConversion) || IsUserDefinedValueTypeConversion(rightConversion, leftConversion))
			{
				// User-defined conversion operator is implicitly converting null into a value type, which will then run the user-defined equality operator.
				// As such there is no compile-time guaranteed result.
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.DontEquateValueTypesWithNullRule,
					expression.GetLocation()));
		}

		static bool IsValueType(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var type = model.GetTypeInfo(expression, cancellationToken).Type;
			return IsValueType(type);
		}

		static bool IsValueType(ITypeSymbol? type) => type != null && type.IsValueType && type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

		static bool IsUserDefinedValueTypeConversion(Conversion first, Conversion second) => first.IsIdentity && IsLoweredToValueType(second);

		static bool IsLoweredToValueType(Conversion conversion) => conversion.IsImplicit && conversion.IsUserDefined && !conversion.IsNullable;
	}
}
