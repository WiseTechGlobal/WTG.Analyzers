using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	partial class EmitMatrix
	{
		public const string Emit = "Emit";
		public const string EmitCall = "EmitCall";
		public const string EmitCalli = "EmitCalli";

		public static bool TryGetEmitMethod(IMethodSymbol method, out EmitMethod emitMethod)
		{
			if (!method.ContainingType.IsMatch("System.Reflection.Emit.ILGenerator"))
			{
				emitMethod = EmitMethod.None;
				return false;
			}

			switch (method.Name)
			{
				case EmitCall:
					emitMethod = EmitMethod.EmitCall;
					break;

				case EmitCalli:
					emitMethod = EmitMethod.EmitCalli;
					break;

				case Emit:
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
				"SignatureHelper" => EmitMethod.Emit_SignatureHelper,
				nameof(ConstructorInfo) => EmitMethod.Emit_ConstructorInfo,
				nameof(Type) => EmitMethod.Emit_Type,
				nameof(Int64) => EmitMethod.Emit_Int64,
				nameof(Single) => EmitMethod.Emit_Single,
				nameof(Double) => EmitMethod.Emit_Double,
				"Label" => EmitMethod.Emit_Label,
				nameof(FieldInfo) => EmitMethod.Emit_FieldInfo,
				nameof(String) => EmitMethod.Emit_String,
				"LocalBuilder" => EmitMethod.Emit_LocalBuilder,
				_ => EmitMethod.None,
			};
		}
	}
}
