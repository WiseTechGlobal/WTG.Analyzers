using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I8, 0x10);
		g.Emit(OpCodes.Ldc_I8, 0b10);
		g.Emit(OpCodes.Ldc_I4, 0x10L);
		g.Emit(OpCodes.Ldc_I4, 0b10L);
	}
}
