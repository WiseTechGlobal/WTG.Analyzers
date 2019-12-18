using System;

namespace WTG.Analyzers
{
	enum OpCodeOperand
	{
		Invalid = 0,
		InlineBrTarget = 1,
		InlineField = 2,
		InlineI = 3,
		InlineI8 = 4,
		InlineMethod = 5,
		InlineNone = 6,
		InlineR = 7,
		InlineSig = 8,
		InlineString = 9,
		InlineSwitch = 10,
		InlineTok = 11,
		InlineType = 12,
		InlineVar = 13,
		ShortInlineBrTarget = 14,
		ShortInlineI = 15,
		ShortInlineR = 16,
		ShortInlineVar = 17,
	}

	[Flags]
	enum EmitMethod
	{
		None = 0,
		Emit = 1 << 0,
		Emit_Byte = 1 << 1,
		Emit_ConstructorInfo = 1 << 2,
		Emit_Double = 1 << 3,
		Emit_FieldInfo = 1 << 4,
		Emit_Int16 = 1 << 5,
		Emit_Int32 = 1 << 6,
		Emit_Int64 = 1 << 7,
		Emit_Label = 1 << 8,
		Emit_LabelArray = 1 << 9,
		Emit_LocalBuilder = 1 << 10,
		Emit_MethodInfo = 1 << 11,
		Emit_SByte = 1 << 12,
		Emit_SignatureHelper = 1 << 13,
		Emit_Single = 1 << 14,
		Emit_String = 1 << 15,
		Emit_Type = 1 << 16,
		EmitCall = 1 << 17,
		EmitCalli = 1 << 18,
	}

	enum OpCode
	{
		Invalid = 0,
		#region InlineBrTarget

		/// <summary>
		/// beq
		/// </summary>
		Beq = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bge
		/// </summary>
		Bge = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bge.un
		/// </summary>
		Bge_Un = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bgt
		/// </summary>
		Bgt = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bgt.un
		/// </summary>
		Bgt_Un = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// ble
		/// </summary>
		Ble = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// ble.un
		/// </summary>
		Ble_Un = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// blt
		/// </summary>
		Blt = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// blt.un
		/// </summary>
		Blt_Un = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bne.un
		/// </summary>
		Bne_Un = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// br
		/// </summary>
		Br = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// brfalse
		/// </summary>
		Brfalse = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// brtrue
		/// </summary>
		Brtrue = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// leave
		/// </summary>
		Leave = OpCodeOperand.InlineBrTarget
			| (EmitMethod.Emit_Label << 5),
		#endregion
		#region InlineField

		/// <summary>
		/// ldfld
		/// </summary>
		Ldfld = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),

		/// <summary>
		/// ldflda
		/// </summary>
		Ldflda = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),

		/// <summary>
		/// ldsfld
		/// </summary>
		Ldsfld = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),

		/// <summary>
		/// ldsflda
		/// </summary>
		Ldsflda = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),

		/// <summary>
		/// stfld
		/// </summary>
		Stfld = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),

		/// <summary>
		/// stsfld
		/// </summary>
		Stsfld = OpCodeOperand.InlineField
			| (EmitMethod.Emit_FieldInfo << 5),
		#endregion
		#region InlineI

		/// <summary>
		/// ldc.i4
		/// </summary>
		Ldc_I4 = OpCodeOperand.InlineI
			| (EmitMethod.Emit_Int32 << 5),
		#endregion
		#region InlineI8

		/// <summary>
		/// ldc.i8
		/// </summary>
		Ldc_I8 = OpCodeOperand.InlineI8
			| (EmitMethod.Emit_Int64 << 5),
		#endregion
		#region InlineMethod

		/// <summary>
		/// call
		/// </summary>
		Call = OpCodeOperand.InlineMethod
			| (EmitMethod.EmitCall << 5)
			| (EmitMethod.Emit_MethodInfo << 5)
			| (EmitMethod.Emit_ConstructorInfo << 5),

		/// <summary>
		/// callvirt
		/// </summary>
		Callvirt = OpCodeOperand.InlineMethod
			| (EmitMethod.EmitCall << 5)
			| (EmitMethod.Emit_MethodInfo << 5),

		/// <summary>
		/// jmp
		/// </summary>
		Jmp = OpCodeOperand.InlineMethod
			| (EmitMethod.Emit_MethodInfo << 5),

		/// <summary>
		/// ldftn
		/// </summary>
		Ldftn = OpCodeOperand.InlineMethod
			| (EmitMethod.Emit_MethodInfo << 5),

		/// <summary>
		/// ldvirtftn
		/// </summary>
		Ldvirtftn = OpCodeOperand.InlineMethod
			| (EmitMethod.Emit_MethodInfo << 5),

		/// <summary>
		/// newobj
		/// </summary>
		Newobj = OpCodeOperand.InlineMethod
			| (EmitMethod.Emit_ConstructorInfo << 5),
		#endregion
		#region InlineNone

		/// <summary>
		/// add
		/// </summary>
		Add = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// add.ovf
		/// </summary>
		Add_Ovf = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// add.ovf.un
		/// </summary>
		Add_Ovf_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// and
		/// </summary>
		And = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// arglist
		/// </summary>
		Arglist = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// break
		/// </summary>
		Break = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ceq
		/// </summary>
		Ceq = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// cgt
		/// </summary>
		Cgt = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// cgt.un
		/// </summary>
		Cgt_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ckfinite
		/// </summary>
		Ckfinite = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// clt
		/// </summary>
		Clt = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// clt.un
		/// </summary>
		Clt_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.i
		/// </summary>
		Conv_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.i1
		/// </summary>
		Conv_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.i2
		/// </summary>
		Conv_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.i4
		/// </summary>
		Conv_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.i8
		/// </summary>
		Conv_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i
		/// </summary>
		Conv_Ovf_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i.un
		/// </summary>
		Conv_Ovf_I_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i1
		/// </summary>
		Conv_Ovf_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i1.un
		/// </summary>
		Conv_Ovf_I1_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i2
		/// </summary>
		Conv_Ovf_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i2.un
		/// </summary>
		Conv_Ovf_I2_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i4
		/// </summary>
		Conv_Ovf_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i4.un
		/// </summary>
		Conv_Ovf_I4_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i8
		/// </summary>
		Conv_Ovf_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.i8.un
		/// </summary>
		Conv_Ovf_I8_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u
		/// </summary>
		Conv_Ovf_U = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u.un
		/// </summary>
		Conv_Ovf_U_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u1
		/// </summary>
		Conv_Ovf_U1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u1.un
		/// </summary>
		Conv_Ovf_U1_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u2
		/// </summary>
		Conv_Ovf_U2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u2.un
		/// </summary>
		Conv_Ovf_U2_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u4
		/// </summary>
		Conv_Ovf_U4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u4.un
		/// </summary>
		Conv_Ovf_U4_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u8
		/// </summary>
		Conv_Ovf_U8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.ovf.u8.un
		/// </summary>
		Conv_Ovf_U8_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.r.un
		/// </summary>
		Conv_R_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.r4
		/// </summary>
		Conv_R4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.r8
		/// </summary>
		Conv_R8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.u
		/// </summary>
		Conv_U = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.u1
		/// </summary>
		Conv_U1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.u2
		/// </summary>
		Conv_U2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.u4
		/// </summary>
		Conv_U4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// conv.u8
		/// </summary>
		Conv_U8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// cpblk
		/// </summary>
		Cpblk = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// div
		/// </summary>
		Div = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// div.un
		/// </summary>
		Div_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// dup
		/// </summary>
		Dup = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// endfilter
		/// </summary>
		Endfilter = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// endfinally
		/// </summary>
		Endfinally = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// initblk
		/// </summary>
		Initblk = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldarg.0
		/// </summary>
		Ldarg_0 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldarg.1
		/// </summary>
		Ldarg_1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldarg.2
		/// </summary>
		Ldarg_2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldarg.3
		/// </summary>
		Ldarg_3 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.0
		/// </summary>
		Ldc_I4_0 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.1
		/// </summary>
		Ldc_I4_1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.2
		/// </summary>
		Ldc_I4_2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.3
		/// </summary>
		Ldc_I4_3 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.4
		/// </summary>
		Ldc_I4_4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.5
		/// </summary>
		Ldc_I4_5 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.6
		/// </summary>
		Ldc_I4_6 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.7
		/// </summary>
		Ldc_I4_7 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.8
		/// </summary>
		Ldc_I4_8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldc.i4.m1
		/// </summary>
		Ldc_I4_M1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.i
		/// </summary>
		Ldelem_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.i1
		/// </summary>
		Ldelem_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.i2
		/// </summary>
		Ldelem_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.i4
		/// </summary>
		Ldelem_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.i8
		/// </summary>
		Ldelem_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.r4
		/// </summary>
		Ldelem_R4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.r8
		/// </summary>
		Ldelem_R8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.ref
		/// </summary>
		Ldelem_Ref = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.u1
		/// </summary>
		Ldelem_U1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.u2
		/// </summary>
		Ldelem_U2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldelem.u4
		/// </summary>
		Ldelem_U4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.i
		/// </summary>
		Ldind_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.i1
		/// </summary>
		Ldind_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.i2
		/// </summary>
		Ldind_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.i4
		/// </summary>
		Ldind_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.i8
		/// </summary>
		Ldind_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.r4
		/// </summary>
		Ldind_R4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.r8
		/// </summary>
		Ldind_R8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.ref
		/// </summary>
		Ldind_Ref = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.u1
		/// </summary>
		Ldind_U1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.u2
		/// </summary>
		Ldind_U2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldind.u4
		/// </summary>
		Ldind_U4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldlen
		/// </summary>
		Ldlen = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldloc.0
		/// </summary>
		Ldloc_0 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldloc.1
		/// </summary>
		Ldloc_1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldloc.2
		/// </summary>
		Ldloc_2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldloc.3
		/// </summary>
		Ldloc_3 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ldnull
		/// </summary>
		Ldnull = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// localloc
		/// </summary>
		Localloc = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// mul
		/// </summary>
		Mul = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// mul.ovf
		/// </summary>
		Mul_Ovf = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// mul.ovf.un
		/// </summary>
		Mul_Ovf_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// neg
		/// </summary>
		Neg = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// nop
		/// </summary>
		Nop = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// not
		/// </summary>
		Not = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// or
		/// </summary>
		Or = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// pop
		/// </summary>
		Pop = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// readonly.
		/// </summary>
		Readonly = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// refanytype
		/// </summary>
		Refanytype = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// rem
		/// </summary>
		Rem = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// rem.un
		/// </summary>
		Rem_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// ret
		/// </summary>
		Ret = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// rethrow
		/// </summary>
		Rethrow = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// shl
		/// </summary>
		Shl = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// shr
		/// </summary>
		Shr = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// shr.un
		/// </summary>
		Shr_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.i
		/// </summary>
		Stelem_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.i1
		/// </summary>
		Stelem_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.i2
		/// </summary>
		Stelem_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.i4
		/// </summary>
		Stelem_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.i8
		/// </summary>
		Stelem_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.r4
		/// </summary>
		Stelem_R4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.r8
		/// </summary>
		Stelem_R8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stelem.ref
		/// </summary>
		Stelem_Ref = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.i
		/// </summary>
		Stind_I = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.i1
		/// </summary>
		Stind_I1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.i2
		/// </summary>
		Stind_I2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.i4
		/// </summary>
		Stind_I4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.i8
		/// </summary>
		Stind_I8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.r4
		/// </summary>
		Stind_R4 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.r8
		/// </summary>
		Stind_R8 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stind.ref
		/// </summary>
		Stind_Ref = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stloc.0
		/// </summary>
		Stloc_0 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stloc.1
		/// </summary>
		Stloc_1 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stloc.2
		/// </summary>
		Stloc_2 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// stloc.3
		/// </summary>
		Stloc_3 = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// sub
		/// </summary>
		Sub = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// sub.ovf
		/// </summary>
		Sub_Ovf = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// sub.ovf.un
		/// </summary>
		Sub_Ovf_Un = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// tail.
		/// </summary>
		Tailcall = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// throw
		/// </summary>
		Throw = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// volatile.
		/// </summary>
		Volatile = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),

		/// <summary>
		/// xor
		/// </summary>
		Xor = OpCodeOperand.InlineNone
			| (EmitMethod.Emit << 5),
		#endregion
		#region InlineR

		/// <summary>
		/// ldc.r8
		/// </summary>
		Ldc_R8 = OpCodeOperand.InlineR
			| (EmitMethod.Emit_Double << 5),
		#endregion
		#region InlineSig

		/// <summary>
		/// calli
		/// </summary>
		Calli = OpCodeOperand.InlineSig
			| (EmitMethod.EmitCalli << 5)
			| (EmitMethod.Emit_SignatureHelper << 5),
		#endregion
		#region InlineString

		/// <summary>
		/// ldstr
		/// </summary>
		Ldstr = OpCodeOperand.InlineString
			| (EmitMethod.Emit_String << 5),
		#endregion
		#region InlineSwitch

		/// <summary>
		/// switch
		/// </summary>
		Switch = OpCodeOperand.InlineSwitch
			| (EmitMethod.Emit_LabelArray << 5),
		#endregion
		#region InlineTok

		/// <summary>
		/// ldtoken
		/// </summary>
		Ldtoken = OpCodeOperand.InlineTok
			| (EmitMethod.Emit_Type << 5)
			| (EmitMethod.Emit_FieldInfo << 5)
			| (EmitMethod.Emit_MethodInfo << 5),
		#endregion
		#region InlineType

		/// <summary>
		/// box
		/// </summary>
		Box = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// castclass
		/// </summary>
		Castclass = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// constrained.
		/// </summary>
		Constrained = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// cpobj
		/// </summary>
		Cpobj = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// initobj
		/// </summary>
		Initobj = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// isinst
		/// </summary>
		Isinst = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// ldelem
		/// </summary>
		Ldelem = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// ldelema
		/// </summary>
		Ldelema = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// ldobj
		/// </summary>
		Ldobj = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// mkrefany
		/// </summary>
		Mkrefany = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// newarr
		/// </summary>
		Newarr = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// refanyval
		/// </summary>
		Refanyval = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// sizeof
		/// </summary>
		Sizeof = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// stelem
		/// </summary>
		Stelem = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// stobj
		/// </summary>
		Stobj = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// unbox
		/// </summary>
		Unbox = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),

		/// <summary>
		/// unbox.any
		/// </summary>
		Unbox_Any = OpCodeOperand.InlineType
			| (EmitMethod.Emit_Type << 5),
		#endregion
		#region InlineVar

		/// <summary>
		/// ldarg
		/// </summary>
		Ldarg = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5),

		/// <summary>
		/// ldarga
		/// </summary>
		Ldarga = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5),

		/// <summary>
		/// ldloc
		/// </summary>
		Ldloc = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),

		/// <summary>
		/// ldloca
		/// </summary>
		Ldloca = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),

		/// <summary>
		/// starg
		/// </summary>
		Starg = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5),

		/// <summary>
		/// stloc
		/// </summary>
		Stloc = OpCodeOperand.InlineVar
			| (EmitMethod.Emit_Int16 << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),
		#endregion
		#region ShortInlineBrTarget

		/// <summary>
		/// beq.s
		/// </summary>
		Beq_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bge.s
		/// </summary>
		Bge_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bge.un.s
		/// </summary>
		Bge_Un_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bgt.s
		/// </summary>
		Bgt_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bgt.un.s
		/// </summary>
		Bgt_Un_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// ble.s
		/// </summary>
		Ble_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// ble.un.s
		/// </summary>
		Ble_Un_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// blt.s
		/// </summary>
		Blt_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// blt.un.s
		/// </summary>
		Blt_Un_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// bne.un.s
		/// </summary>
		Bne_Un_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// br.s
		/// </summary>
		Br_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// brfalse.s
		/// </summary>
		Brfalse_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// brtrue.s
		/// </summary>
		Brtrue_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),

		/// <summary>
		/// leave.s
		/// </summary>
		Leave_S = OpCodeOperand.ShortInlineBrTarget
			| (EmitMethod.Emit_Label << 5),
		#endregion
		#region ShortInlineI

		/// <summary>
		/// ldc.i4.s
		/// </summary>
		Ldc_I4_S = OpCodeOperand.ShortInlineI
			| (EmitMethod.Emit_SByte << 5),

		/// <summary>
		/// unaligned.
		/// </summary>
		Unaligned = OpCodeOperand.ShortInlineI
			| (EmitMethod.Emit_SByte << 5),
		#endregion
		#region ShortInlineR

		/// <summary>
		/// ldc.r4
		/// </summary>
		Ldc_R4 = OpCodeOperand.ShortInlineR
			| (EmitMethod.Emit_Single << 5),
		#endregion
		#region ShortInlineVar

		/// <summary>
		/// ldarg.s
		/// </summary>
		Ldarg_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5),

		/// <summary>
		/// ldarga.s
		/// </summary>
		Ldarga_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5),

		/// <summary>
		/// ldloc.s
		/// </summary>
		Ldloc_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),

		/// <summary>
		/// ldloca.s
		/// </summary>
		Ldloca_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),

		/// <summary>
		/// starg.s
		/// </summary>
		Starg_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5),

		/// <summary>
		/// stloc.s
		/// </summary>
		Stloc_S = OpCodeOperand.ShortInlineVar
			| (EmitMethod.Emit_Byte << 5)
			| (EmitMethod.Emit_LocalBuilder << 5),
		#endregion
	}

	static partial class EmitMatrix
	{
		public static OpCodeOperand GetOperand(this OpCode opCode) => (OpCodeOperand)((int)opCode & 0x1F);
		public static EmitMethod GetSupportedMethods(this OpCode opCode) => (EmitMethod)((int)opCode >> 5 & 0x7FFFF);

		public static OpCode GetOpCode(string field)
		{
			return field switch
			{
				nameof(OpCode.Add) => OpCode.Add,
				nameof(OpCode.Add_Ovf) => OpCode.Add_Ovf,
				nameof(OpCode.Add_Ovf_Un) => OpCode.Add_Ovf_Un,
				nameof(OpCode.And) => OpCode.And,
				nameof(OpCode.Arglist) => OpCode.Arglist,
				nameof(OpCode.Beq) => OpCode.Beq,
				nameof(OpCode.Beq_S) => OpCode.Beq_S,
				nameof(OpCode.Bge) => OpCode.Bge,
				nameof(OpCode.Bge_S) => OpCode.Bge_S,
				nameof(OpCode.Bge_Un) => OpCode.Bge_Un,
				nameof(OpCode.Bge_Un_S) => OpCode.Bge_Un_S,
				nameof(OpCode.Bgt) => OpCode.Bgt,
				nameof(OpCode.Bgt_S) => OpCode.Bgt_S,
				nameof(OpCode.Bgt_Un) => OpCode.Bgt_Un,
				nameof(OpCode.Bgt_Un_S) => OpCode.Bgt_Un_S,
				nameof(OpCode.Ble) => OpCode.Ble,
				nameof(OpCode.Ble_S) => OpCode.Ble_S,
				nameof(OpCode.Ble_Un) => OpCode.Ble_Un,
				nameof(OpCode.Ble_Un_S) => OpCode.Ble_Un_S,
				nameof(OpCode.Blt) => OpCode.Blt,
				nameof(OpCode.Blt_S) => OpCode.Blt_S,
				nameof(OpCode.Blt_Un) => OpCode.Blt_Un,
				nameof(OpCode.Blt_Un_S) => OpCode.Blt_Un_S,
				nameof(OpCode.Bne_Un) => OpCode.Bne_Un,
				nameof(OpCode.Bne_Un_S) => OpCode.Bne_Un_S,
				nameof(OpCode.Box) => OpCode.Box,
				nameof(OpCode.Br) => OpCode.Br,
				nameof(OpCode.Br_S) => OpCode.Br_S,
				nameof(OpCode.Break) => OpCode.Break,
				nameof(OpCode.Brfalse) => OpCode.Brfalse,
				nameof(OpCode.Brfalse_S) => OpCode.Brfalse_S,
				nameof(OpCode.Brtrue) => OpCode.Brtrue,
				nameof(OpCode.Brtrue_S) => OpCode.Brtrue_S,
				nameof(OpCode.Call) => OpCode.Call,
				nameof(OpCode.Calli) => OpCode.Calli,
				nameof(OpCode.Callvirt) => OpCode.Callvirt,
				nameof(OpCode.Castclass) => OpCode.Castclass,
				nameof(OpCode.Ceq) => OpCode.Ceq,
				nameof(OpCode.Cgt) => OpCode.Cgt,
				nameof(OpCode.Cgt_Un) => OpCode.Cgt_Un,
				nameof(OpCode.Ckfinite) => OpCode.Ckfinite,
				nameof(OpCode.Clt) => OpCode.Clt,
				nameof(OpCode.Clt_Un) => OpCode.Clt_Un,
				nameof(OpCode.Constrained) => OpCode.Constrained,
				nameof(OpCode.Conv_I) => OpCode.Conv_I,
				nameof(OpCode.Conv_I1) => OpCode.Conv_I1,
				nameof(OpCode.Conv_I2) => OpCode.Conv_I2,
				nameof(OpCode.Conv_I4) => OpCode.Conv_I4,
				nameof(OpCode.Conv_I8) => OpCode.Conv_I8,
				nameof(OpCode.Conv_Ovf_I) => OpCode.Conv_Ovf_I,
				nameof(OpCode.Conv_Ovf_I_Un) => OpCode.Conv_Ovf_I_Un,
				nameof(OpCode.Conv_Ovf_I1) => OpCode.Conv_Ovf_I1,
				nameof(OpCode.Conv_Ovf_I1_Un) => OpCode.Conv_Ovf_I1_Un,
				nameof(OpCode.Conv_Ovf_I2) => OpCode.Conv_Ovf_I2,
				nameof(OpCode.Conv_Ovf_I2_Un) => OpCode.Conv_Ovf_I2_Un,
				nameof(OpCode.Conv_Ovf_I4) => OpCode.Conv_Ovf_I4,
				nameof(OpCode.Conv_Ovf_I4_Un) => OpCode.Conv_Ovf_I4_Un,
				nameof(OpCode.Conv_Ovf_I8) => OpCode.Conv_Ovf_I8,
				nameof(OpCode.Conv_Ovf_I8_Un) => OpCode.Conv_Ovf_I8_Un,
				nameof(OpCode.Conv_Ovf_U) => OpCode.Conv_Ovf_U,
				nameof(OpCode.Conv_Ovf_U_Un) => OpCode.Conv_Ovf_U_Un,
				nameof(OpCode.Conv_Ovf_U1) => OpCode.Conv_Ovf_U1,
				nameof(OpCode.Conv_Ovf_U1_Un) => OpCode.Conv_Ovf_U1_Un,
				nameof(OpCode.Conv_Ovf_U2) => OpCode.Conv_Ovf_U2,
				nameof(OpCode.Conv_Ovf_U2_Un) => OpCode.Conv_Ovf_U2_Un,
				nameof(OpCode.Conv_Ovf_U4) => OpCode.Conv_Ovf_U4,
				nameof(OpCode.Conv_Ovf_U4_Un) => OpCode.Conv_Ovf_U4_Un,
				nameof(OpCode.Conv_Ovf_U8) => OpCode.Conv_Ovf_U8,
				nameof(OpCode.Conv_Ovf_U8_Un) => OpCode.Conv_Ovf_U8_Un,
				nameof(OpCode.Conv_R_Un) => OpCode.Conv_R_Un,
				nameof(OpCode.Conv_R4) => OpCode.Conv_R4,
				nameof(OpCode.Conv_R8) => OpCode.Conv_R8,
				nameof(OpCode.Conv_U) => OpCode.Conv_U,
				nameof(OpCode.Conv_U1) => OpCode.Conv_U1,
				nameof(OpCode.Conv_U2) => OpCode.Conv_U2,
				nameof(OpCode.Conv_U4) => OpCode.Conv_U4,
				nameof(OpCode.Conv_U8) => OpCode.Conv_U8,
				nameof(OpCode.Cpblk) => OpCode.Cpblk,
				nameof(OpCode.Cpobj) => OpCode.Cpobj,
				nameof(OpCode.Div) => OpCode.Div,
				nameof(OpCode.Div_Un) => OpCode.Div_Un,
				nameof(OpCode.Dup) => OpCode.Dup,
				nameof(OpCode.Endfilter) => OpCode.Endfilter,
				nameof(OpCode.Endfinally) => OpCode.Endfinally,
				nameof(OpCode.Initblk) => OpCode.Initblk,
				nameof(OpCode.Initobj) => OpCode.Initobj,
				nameof(OpCode.Isinst) => OpCode.Isinst,
				nameof(OpCode.Jmp) => OpCode.Jmp,
				nameof(OpCode.Ldarg) => OpCode.Ldarg,
				nameof(OpCode.Ldarg_0) => OpCode.Ldarg_0,
				nameof(OpCode.Ldarg_1) => OpCode.Ldarg_1,
				nameof(OpCode.Ldarg_2) => OpCode.Ldarg_2,
				nameof(OpCode.Ldarg_3) => OpCode.Ldarg_3,
				nameof(OpCode.Ldarg_S) => OpCode.Ldarg_S,
				nameof(OpCode.Ldarga) => OpCode.Ldarga,
				nameof(OpCode.Ldarga_S) => OpCode.Ldarga_S,
				nameof(OpCode.Ldc_I4) => OpCode.Ldc_I4,
				nameof(OpCode.Ldc_I4_0) => OpCode.Ldc_I4_0,
				nameof(OpCode.Ldc_I4_1) => OpCode.Ldc_I4_1,
				nameof(OpCode.Ldc_I4_2) => OpCode.Ldc_I4_2,
				nameof(OpCode.Ldc_I4_3) => OpCode.Ldc_I4_3,
				nameof(OpCode.Ldc_I4_4) => OpCode.Ldc_I4_4,
				nameof(OpCode.Ldc_I4_5) => OpCode.Ldc_I4_5,
				nameof(OpCode.Ldc_I4_6) => OpCode.Ldc_I4_6,
				nameof(OpCode.Ldc_I4_7) => OpCode.Ldc_I4_7,
				nameof(OpCode.Ldc_I4_8) => OpCode.Ldc_I4_8,
				nameof(OpCode.Ldc_I4_M1) => OpCode.Ldc_I4_M1,
				nameof(OpCode.Ldc_I4_S) => OpCode.Ldc_I4_S,
				nameof(OpCode.Ldc_I8) => OpCode.Ldc_I8,
				nameof(OpCode.Ldc_R4) => OpCode.Ldc_R4,
				nameof(OpCode.Ldc_R8) => OpCode.Ldc_R8,
				nameof(OpCode.Ldelem) => OpCode.Ldelem,
				nameof(OpCode.Ldelem_I) => OpCode.Ldelem_I,
				nameof(OpCode.Ldelem_I1) => OpCode.Ldelem_I1,
				nameof(OpCode.Ldelem_I2) => OpCode.Ldelem_I2,
				nameof(OpCode.Ldelem_I4) => OpCode.Ldelem_I4,
				nameof(OpCode.Ldelem_I8) => OpCode.Ldelem_I8,
				nameof(OpCode.Ldelem_R4) => OpCode.Ldelem_R4,
				nameof(OpCode.Ldelem_R8) => OpCode.Ldelem_R8,
				nameof(OpCode.Ldelem_Ref) => OpCode.Ldelem_Ref,
				nameof(OpCode.Ldelem_U1) => OpCode.Ldelem_U1,
				nameof(OpCode.Ldelem_U2) => OpCode.Ldelem_U2,
				nameof(OpCode.Ldelem_U4) => OpCode.Ldelem_U4,
				nameof(OpCode.Ldelema) => OpCode.Ldelema,
				nameof(OpCode.Ldfld) => OpCode.Ldfld,
				nameof(OpCode.Ldflda) => OpCode.Ldflda,
				nameof(OpCode.Ldftn) => OpCode.Ldftn,
				nameof(OpCode.Ldind_I) => OpCode.Ldind_I,
				nameof(OpCode.Ldind_I1) => OpCode.Ldind_I1,
				nameof(OpCode.Ldind_I2) => OpCode.Ldind_I2,
				nameof(OpCode.Ldind_I4) => OpCode.Ldind_I4,
				nameof(OpCode.Ldind_I8) => OpCode.Ldind_I8,
				nameof(OpCode.Ldind_R4) => OpCode.Ldind_R4,
				nameof(OpCode.Ldind_R8) => OpCode.Ldind_R8,
				nameof(OpCode.Ldind_Ref) => OpCode.Ldind_Ref,
				nameof(OpCode.Ldind_U1) => OpCode.Ldind_U1,
				nameof(OpCode.Ldind_U2) => OpCode.Ldind_U2,
				nameof(OpCode.Ldind_U4) => OpCode.Ldind_U4,
				nameof(OpCode.Ldlen) => OpCode.Ldlen,
				nameof(OpCode.Ldloc) => OpCode.Ldloc,
				nameof(OpCode.Ldloc_0) => OpCode.Ldloc_0,
				nameof(OpCode.Ldloc_1) => OpCode.Ldloc_1,
				nameof(OpCode.Ldloc_2) => OpCode.Ldloc_2,
				nameof(OpCode.Ldloc_3) => OpCode.Ldloc_3,
				nameof(OpCode.Ldloc_S) => OpCode.Ldloc_S,
				nameof(OpCode.Ldloca) => OpCode.Ldloca,
				nameof(OpCode.Ldloca_S) => OpCode.Ldloca_S,
				nameof(OpCode.Ldnull) => OpCode.Ldnull,
				nameof(OpCode.Ldobj) => OpCode.Ldobj,
				nameof(OpCode.Ldsfld) => OpCode.Ldsfld,
				nameof(OpCode.Ldsflda) => OpCode.Ldsflda,
				nameof(OpCode.Ldstr) => OpCode.Ldstr,
				nameof(OpCode.Ldtoken) => OpCode.Ldtoken,
				nameof(OpCode.Ldvirtftn) => OpCode.Ldvirtftn,
				nameof(OpCode.Leave) => OpCode.Leave,
				nameof(OpCode.Leave_S) => OpCode.Leave_S,
				nameof(OpCode.Localloc) => OpCode.Localloc,
				nameof(OpCode.Mkrefany) => OpCode.Mkrefany,
				nameof(OpCode.Mul) => OpCode.Mul,
				nameof(OpCode.Mul_Ovf) => OpCode.Mul_Ovf,
				nameof(OpCode.Mul_Ovf_Un) => OpCode.Mul_Ovf_Un,
				nameof(OpCode.Neg) => OpCode.Neg,
				nameof(OpCode.Newarr) => OpCode.Newarr,
				nameof(OpCode.Newobj) => OpCode.Newobj,
				nameof(OpCode.Nop) => OpCode.Nop,
				nameof(OpCode.Not) => OpCode.Not,
				nameof(OpCode.Or) => OpCode.Or,
				nameof(OpCode.Pop) => OpCode.Pop,
				nameof(OpCode.Readonly) => OpCode.Readonly,
				nameof(OpCode.Refanytype) => OpCode.Refanytype,
				nameof(OpCode.Refanyval) => OpCode.Refanyval,
				nameof(OpCode.Rem) => OpCode.Rem,
				nameof(OpCode.Rem_Un) => OpCode.Rem_Un,
				nameof(OpCode.Ret) => OpCode.Ret,
				nameof(OpCode.Rethrow) => OpCode.Rethrow,
				nameof(OpCode.Shl) => OpCode.Shl,
				nameof(OpCode.Shr) => OpCode.Shr,
				nameof(OpCode.Shr_Un) => OpCode.Shr_Un,
				nameof(OpCode.Sizeof) => OpCode.Sizeof,
				nameof(OpCode.Starg) => OpCode.Starg,
				nameof(OpCode.Starg_S) => OpCode.Starg_S,
				nameof(OpCode.Stelem) => OpCode.Stelem,
				nameof(OpCode.Stelem_I) => OpCode.Stelem_I,
				nameof(OpCode.Stelem_I1) => OpCode.Stelem_I1,
				nameof(OpCode.Stelem_I2) => OpCode.Stelem_I2,
				nameof(OpCode.Stelem_I4) => OpCode.Stelem_I4,
				nameof(OpCode.Stelem_I8) => OpCode.Stelem_I8,
				nameof(OpCode.Stelem_R4) => OpCode.Stelem_R4,
				nameof(OpCode.Stelem_R8) => OpCode.Stelem_R8,
				nameof(OpCode.Stelem_Ref) => OpCode.Stelem_Ref,
				nameof(OpCode.Stfld) => OpCode.Stfld,
				nameof(OpCode.Stind_I) => OpCode.Stind_I,
				nameof(OpCode.Stind_I1) => OpCode.Stind_I1,
				nameof(OpCode.Stind_I2) => OpCode.Stind_I2,
				nameof(OpCode.Stind_I4) => OpCode.Stind_I4,
				nameof(OpCode.Stind_I8) => OpCode.Stind_I8,
				nameof(OpCode.Stind_R4) => OpCode.Stind_R4,
				nameof(OpCode.Stind_R8) => OpCode.Stind_R8,
				nameof(OpCode.Stind_Ref) => OpCode.Stind_Ref,
				nameof(OpCode.Stloc) => OpCode.Stloc,
				nameof(OpCode.Stloc_0) => OpCode.Stloc_0,
				nameof(OpCode.Stloc_1) => OpCode.Stloc_1,
				nameof(OpCode.Stloc_2) => OpCode.Stloc_2,
				nameof(OpCode.Stloc_3) => OpCode.Stloc_3,
				nameof(OpCode.Stloc_S) => OpCode.Stloc_S,
				nameof(OpCode.Stobj) => OpCode.Stobj,
				nameof(OpCode.Stsfld) => OpCode.Stsfld,
				nameof(OpCode.Sub) => OpCode.Sub,
				nameof(OpCode.Sub_Ovf) => OpCode.Sub_Ovf,
				nameof(OpCode.Sub_Ovf_Un) => OpCode.Sub_Ovf_Un,
				nameof(OpCode.Switch) => OpCode.Switch,
				nameof(OpCode.Tailcall) => OpCode.Tailcall,
				nameof(OpCode.Throw) => OpCode.Throw,
				nameof(OpCode.Unaligned) => OpCode.Unaligned,
				nameof(OpCode.Unbox) => OpCode.Unbox,
				nameof(OpCode.Unbox_Any) => OpCode.Unbox_Any,
				nameof(OpCode.Volatile) => OpCode.Volatile,
				nameof(OpCode.Xor) => OpCode.Xor,
				_ => OpCode.Invalid,
			};
		}
	}
}
