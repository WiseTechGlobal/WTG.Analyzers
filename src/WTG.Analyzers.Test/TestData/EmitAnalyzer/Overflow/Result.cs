using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g)
	{
		g.Emit(OpCodes.Ldc_I4, (int)0xffffffff);
	}
}
