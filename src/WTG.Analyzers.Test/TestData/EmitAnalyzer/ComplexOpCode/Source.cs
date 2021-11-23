using System.Reflection.Emit;

class Test
{
	public void Method(ILGenerator g, bool shortForm, Label label) => g.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
}
