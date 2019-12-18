using System.Reflection.Emit;

class Test
{
	public void Literal(ILGenerator g)
	{
		g.Emit(OpCodes.Ldarg_0, 0);
		g.Emit(OpCodes.Ldarg_S, 8);
		g.Emit(OpCodes.Ldarg, 0);

		g.Emit(OpCodes.Ldc_I4_S, 8);
		g.Emit(OpCodes.Ldc_I8, 8);
		g.Emit(OpCodes.Ldc_R4, 8);
		g.Emit(OpCodes.Ldc_R8, 8);
	}

	public void Variable(ILGenerator g, int value)
	{
		g.Emit(OpCodes.Ldarg_S, value);
		g.Emit(OpCodes.Ldarg, value);

		g.Emit(OpCodes.Ldc_I4_S, value);
		g.Emit(OpCodes.Ldc_I8, value);
		g.Emit(OpCodes.Ldc_R4, value);
		g.Emit(OpCodes.Ldc_R8, value);
	}
}
