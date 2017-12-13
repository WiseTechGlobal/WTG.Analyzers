public static class Foo
{
	public static void StaticArrayCreator<T>()
	{
		var o = new object[1];
		var s = new string[2];
		var i = new int[] { 8 };
		var ni = new int?[] { 6 };

		var t = new T[4];
	}

	public static void DynamicArrayCreator<T>(int size = 0)
	{
		int GetSize() => 0;

		const int Size = 0;
		var o1 = new object[Size];
		var o2 = new object[size];
		var o3 = new object[GetSize()];
		var s1 = new string[Size];
		var s2 = new string[size];
		var s3 = new string[GetSize()];
		var i1 = new int[Size];
		var i2 = new int[size];
		var i3 = new int[GetSize()];
		var ni1 = new int?[Size];
		var ni2 = new int?[size];
		var ni3 = new int?[GetSize()];

		var t1 = new T[Size];
		var t2 = new T[size];
		var t3 = new T[GetSize()];
	}
}
