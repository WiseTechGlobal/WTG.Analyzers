using System;

public class Bob
{
	public void ForLoop(int[] values)
	{
		for (var i = 0; i < values.Length; i++)
		{
		}
	}

	public void ForLoopWithoutLocal(Type type)
	{
		for (; type.IsArray; type = type.GetElementType())
		{
		}
	}
}
