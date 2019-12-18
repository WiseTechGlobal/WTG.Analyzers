using System;
using System.Reflection;
using System.Reflection.Emit;

class Test
{
	public void Constants(ILGenerator g)
	{
		g.Emit(OpCodes.Ldarg_0);
		g.Emit(OpCodes.Ldarg_S, (byte)8);
		g.Emit(OpCodes.Ldarg, (short)0);

		g.Emit(OpCodes.Ldc_I4_S, (sbyte)8);
		g.Emit(OpCodes.Ldc_I4, 8);
		g.Emit(OpCodes.Ldc_I8, 8L);
		g.Emit(OpCodes.Ldc_R4, 8f);
		g.Emit(OpCodes.Ldc_R8, 8d);
	}

	public void Labels(ILGenerator g)
	{
		g.Emit(OpCodes.Switch, new Label[] { g.DefineLabel(), g.DefineLabel() });
		g.Emit(OpCodes.Br, g.DefineLabel());
		g.Emit(OpCodes.Br_S, g.DefineLabel());
	}

	public void Locals(ILGenerator g)
	{
		g.Emit(OpCodes.Ldloc, (short)0);
		g.Emit(OpCodes.Ldloc_S, (byte)0);
		g.Emit(OpCodes.Stloc, g.DeclareLocal(typeof(int)));
		g.Emit(OpCodes.Stloc_S, g.DeclareLocal(typeof(int)));
	}

	public void MethodInfo(ILGenerator g, MethodInfo info)
	{
		g.Emit(OpCodes.Call, info);
		g.Emit(OpCodes.Callvirt, info);
		g.Emit(OpCodes.Jmp, info);
		g.EmitCall(OpCodes.Call, info, Type.EmptyTypes);
		g.EmitCall(OpCodes.Callvirt, info, Type.EmptyTypes);
	}

	public void ConstructorInfo(ILGenerator g, ConstructorInfo info)
	{
		g.Emit(OpCodes.Call, info); // needed for an emitted constructor to call the base constructor.
		g.Emit(OpCodes.Newobj, info);
	}

	public void FieldInfo(ILGenerator g, FieldInfo info)
	{
		g.Emit(OpCodes.Ldfld, info);
		g.Emit(OpCodes.Stfld, info);
		g.Emit(OpCodes.Ldflda, info);
	}

	public void Signature(ILGenerator g, SignatureHelper sig)
	{
		g.Emit(OpCodes.Calli, sig);
	}
}
