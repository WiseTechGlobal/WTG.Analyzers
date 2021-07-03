using System.Reflection.Emit;

class Test
{
	public void Literal(ILGenerator g)
	{
		g.Emit(OpCodes.Ldarg_0);
		g.Emit(OpCodes.Ldarg_S, (byte)8);
		g.Emit(OpCodes.Ldarg, (short)0);

		g.Emit(OpCodes.Ldc_I4_S, (sbyte)8);
		g.Emit(OpCodes.Ldc_I8, 8L);
		g.Emit(OpCodes.Ldc_R4, 8f);
		g.Emit(OpCodes.Ldc_R8, 8d);
	}

	public void Variable(ILGenerator g, int value)
	{
		g.Emit(OpCodes.Ldarg_S, (byte)value);
		g.Emit(OpCodes.Ldarg, (short)value);

		g.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
		g.Emit(OpCodes.Ldc_I8, (long)value);
		g.Emit(OpCodes.Ldc_R4, (float)value);
		g.Emit(OpCodes.Ldc_R8, (double)value);
	}
}
