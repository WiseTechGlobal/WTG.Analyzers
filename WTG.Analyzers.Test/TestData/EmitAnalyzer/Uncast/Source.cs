using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I4, (byte)8);
		g.Emit(OpCodes.Ldc_I8, (byte)8);
		g.Emit(OpCodes.Ldc_R4, (byte)8);
		g.Emit(OpCodes.Ldc_R8, (byte)8);
	}
}
