using System;

public static class Foo
{
	public static void ArrayCreator()
	{
		var o = new object[][][] { };
		var s = new string[][] { };
		var i = new int[][] { };
		var ni = new int?[][] { };

		var i2 = new int[][] { new int[6] };
		var i3 = new int[][] { new int[] { 9 } };
	}

	public static void GenericArrayCreator<T>()
	{
		var t = new T[][] { };
	}

#pragma warning disable CS0078

	public static int[][] M00() => new int[0][];
	public static int[][] M01() => new int[0x0][];
	public static int[][] M02() => new int[0u][];
	public static int[][] M03() => new int[0l][];
	public static int[][] M04() => new int[0L][];
	public static int[][] M05() => new int[0lu][];
	public static int[][] M06() => new int[0LU][];
	public static int[][] M07() => new int['\0'][];
	public static int[][] M08() => new int[][] { };
	public static int[][] M09() => new int[0b0000___0000_000][];
	public static int[][] M10() => new int[(short)0][];
	public static int[][] M11() => new int[(ushort)0][];
	public static int[][] M12() => new int[(byte)0][];
	public static int[][] M13() => new int[(sbyte)0][];
	public static int[][] M14() => new int[00][];
	public static int[][] M15() => new int[-0][];
	public static int[][] M16() => new int[+0][];
	public static int[][] E() => Array.Empty<int[]>();

#pragma warning restore CS0078
}
