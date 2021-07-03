using System.Reflection;
using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g, MethodInfo info)
	{
		g.Emit(OpCodes.Ldarg_S, info);
		g.Emit(OpCodes.Newarr, info);
		g.Emit(OpCodes.Ldloc, info);
		g.Emit(OpCodes.Calli, info);
	}
}
