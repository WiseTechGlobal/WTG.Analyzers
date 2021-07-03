using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I8, 16L);
		g.Emit(OpCodes.Ldc_I8, 2L);
		g.Emit(OpCodes.Ldc_I4, 16);
		g.Emit(OpCodes.Ldc_I4, 2);
	}
}
