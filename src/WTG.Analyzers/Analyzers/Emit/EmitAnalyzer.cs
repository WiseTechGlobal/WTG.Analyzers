using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class EmitAnalyzer : DiagnosticAnalyzer
	{
		public const string SuggestedFixProperty = "SuggestedFix";
		public const string DeleteArgument = "D";
		public const string ConvertArgument = "C";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UseCorrectEmitOverloadRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeInvoke, SyntaxKind.InvocationExpression);
		}

		static void AnalyzeInvoke(SyntaxNodeAnalysisContext context)
		{
			var invoke = (InvocationExpressionSyntax)context.Node;

			if (TryIdentifyEmitMethod(context.SemanticModel, invoke, out var actualEmitMethod, context.CancellationToken) &&
				TryGetOpCodeFieldFromEmitCall(context.SemanticModel, invoke, out var opCodeSymbol, context.CancellationToken) &&
				EmitMatrix.GetOpCode(opCodeSymbol) is var opcode &&
				AreCompatible(opcode, actualEmitMethod))
			{
				var properties = ImmutableDictionary<string, string>.Empty;
				DiagnosticDescriptor descriptor;

				var operandType = opcode.GetOperand();

				if (operandType == OpCodeOperand.InlineNone)
				{
					descriptor = Rules.UseCorrectEmitOverload_NoneRule;
					properties = properties.Add(SuggestedFixProperty, DeleteArgument);
				}
				else
				{
					descriptor = Rules.UseCorrectEmitOverloadRule;

					if (CanConvert(operandType, actualEmitMethod))
					{
						properties = properties.Add(SuggestedFixProperty, ConvertArgument);
					}
				}

				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor,
						invoke.GetLocation(),
						properties,
						opCodeSymbol.Name));
			}
		}

		static bool TryIdentifyEmitMethod(SemanticModel model, InvocationExpressionSyntax invoke, out EmitMethod method, CancellationToken cancellationToken)
		{
			var name = ExpressionHelper.GetMethodName(invoke)?.Identifier.Text;

			if (name == EmitMatrix.Emit || name == EmitMatrix.EmitCall || name == EmitMatrix.EmitCalli)
			{
				var methodSymbol = (IMethodSymbol)model.GetSymbolInfo(invoke, cancellationToken).Symbol;

				if (methodSymbol != null)
				{
					return EmitMatrix.TryGetEmitMethod(methodSymbol, out method);
				}
			}

			method = EmitMethod.None;
			return false;
		}

		static bool TryGetOpCodeFieldFromEmitCall(SemanticModel model, InvocationExpressionSyntax emitCall, [NotNullWhen(true)] out IFieldSymbol? opCodeSymbol, CancellationToken cancellation)
		{
			if (emitCall.ArgumentList.Arguments[0].Accept(FieldAccessor.Instance) is var fieldIdentifier &&
				model.GetSymbolInfo(fieldIdentifier, cancellation).Symbol is var fieldSymbol &&
				fieldSymbol.Kind == SymbolKind.Field)
			{
				opCodeSymbol = (IFieldSymbol)fieldSymbol;
				return true;
			}

			opCodeSymbol = null;
			return false;
		}

		static bool AreCompatible(OpCode opcode, EmitMethod emitMethod)
			=> opcode != OpCode.Invalid && (EmitMatrix.GetSupportedMethods(opcode) & emitMethod) == 0;

		static bool CanConvert(OpCodeOperand requiredOperand, EmitMethod actualMethod)
		{
			switch (requiredOperand)
			{
				case OpCodeOperand.InlineI:
				case OpCodeOperand.InlineI8:
				case OpCodeOperand.InlineR:
				case OpCodeOperand.InlineVar:
				case OpCodeOperand.ShortInlineI:
				case OpCodeOperand.ShortInlineR:
				case OpCodeOperand.ShortInlineVar:
					break;

				default:
					return false;
			}

			const EmitMethod Mask =
				EmitMethod.Emit_Byte
				| EmitMethod.Emit_SByte
				| EmitMethod.Emit_Int16
				| EmitMethod.Emit_Int32
				| EmitMethod.Emit_Int64
				| EmitMethod.Emit_Single
				| EmitMethod.Emit_Double;

			return (Mask & actualMethod) != 0;
		}
	}
}
