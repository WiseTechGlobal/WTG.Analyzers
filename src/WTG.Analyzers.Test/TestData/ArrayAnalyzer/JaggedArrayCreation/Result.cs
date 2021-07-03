using System;

public static class Foo
{
	public static void ArrayCreator()
	{
		var o = Array.Empty<object[][]>();
		var s = Array.Empty<string[]>();
		var i = Array.Empty<int[]>();
		var ni = Array.Empty<int?[]>();

		var i2 = new int[][] { new int[6] };
		var i3 = new int[][] { new int[] { 9 } };
	}

	public static void GenericArrayCreator<T>()
	{
		var t = Array.Empty<T[]>();
	}

#pragma warning disable CS0078

	public static int[][] M00() => Array.Empty<int[]>();
	public static int[][] M01() => Array.Empty<int[]>();
	public static int[][] M02() => Array.Empty<int[]>();
	public static int[][] M03() => Array.Empty<int[]>();
	public static int[][] M04() => Array.Empty<int[]>();
	public static int[][] M05() => Array.Empty<int[]>();
	public static int[][] M06() => Array.Empty<int[]>();
	public static int[][] M07() => Array.Empty<int[]>();
	public static int[][] M08() => Array.Empty<int[]>();
	public static int[][] M09() => Array.Empty<int[]>();
	public static int[][] M10() => Array.Empty<int[]>();
	public static int[][] M11() => Array.Empty<int[]>();
	public static int[][] M12() => Array.Empty<int[]>();
	public static int[][] M13() => Array.Empty<int[]>();
	public static int[][] M14() => Array.Empty<int[]>();
	public static int[][] M15() => Array.Empty<int[]>();
	public static int[][] M16() => Array.Empty<int[]>();
	public static int[][] E() => Array.Empty<int[]>();

#pragma warning restore CS0078
}
