using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I4, 2 + 3);
		g.Emit(OpCodes.Ldc_I8, 2 + 3);
	}
}
