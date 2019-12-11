using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I4, 8);
		g.Emit(OpCodes.Ldc_I8, 8L);
		g.Emit(OpCodes.Ldc_R4, 8f);
		g.Emit(OpCodes.Ldc_R8, 8d);
	}
}
