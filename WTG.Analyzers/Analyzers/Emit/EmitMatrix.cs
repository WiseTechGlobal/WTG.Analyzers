using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CodeAnalysis;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	partial class EmitMatrix
	{
		public static bool TryGetEmitMethod(IMethodSymbol method, out EmitMethod emitMethod)
		{
			if (!method.ContainingType.IsMatch("System.Reflection.Emit.ILGenerator"))
			{
				emitMethod = EmitMethod.None;
				return false;
			}

			switch (method.Name)
			{
				case nameof(ILGenerator.EmitCall):
					emitMethod = EmitMethod.EmitCall;
					break;

				case nameof(ILGenerator.EmitCalli):
					emitMethod = EmitMethod.EmitCalli;
					break;

				case nameof(ILGenerator.Emit):
					emitMethod = GetPlainEmitMethod(method);
					break;

				default:
					emitMethod = EmitMethod.None;
					return false;
			}

			return true;
		}

		public static OpCode GetOpCode(IFieldSymbol field)
		{
			if (field.ContainingType.IsMatch("System.Reflection.Emit.OpCodes"))
			{
				return GetOpCode(field.Name);
			}

			return OpCode.Invalid;
		}

		static EmitMethod GetPlainEmitMethod(IMethodSymbol method)
		{
			if (method.Parameters.Length == 1)
			{
				return EmitMethod.Emit;
			}

			var argType = method.Parameters[1].Type;

			if (argType.Kind == SymbolKind.ArrayType)
			{
				return EmitMethod.Emit_LabelArray;
			}

			return argType.Name switch
			{
				nameof(Byte) => EmitMethod.Emit_Byte,
				nameof(SByte) => EmitMethod.Emit_SByte,
				nameof(Int16) => EmitMethod.Emit_Int16,
				nameof(Int32) => EmitMethod.Emit_Int32,
				nameof(MethodInfo) => EmitMethod.Emit_MethodInfo,
				nameof(SignatureHelper) => EmitMethod.Emit_SignatureHelper,
				nameof(ConstructorInfo) => EmitMethod.Emit_ConstructorInfo,
				nameof(Type) => EmitMethod.Emit_Type,
				nameof(Int64) => EmitMethod.Emit_Int64,
				nameof(Single) => EmitMethod.Emit_Single,
				nameof(Double) => EmitMethod.Emit_Double,
				nameof(Label) => EmitMethod.Emit_Label,
				nameof(FieldInfo) => EmitMethod.Emit_FieldInfo,
				nameof(String) => EmitMethod.Emit_String,
				nameof(LocalBuilder) => EmitMethod.Emit_LocalBuilder,
				_ => EmitMethod.None,
			};
		}
	}
}
