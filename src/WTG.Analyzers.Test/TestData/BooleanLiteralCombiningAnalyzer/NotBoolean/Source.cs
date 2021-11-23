using System.Diagnostics.Contracts;

public static class Bob
{
	public static bool? Complex1(bool a) => a ? true : (bool?)null;
	public static bool? Complex2(bool a) => a ? (bool?)null : true;
	public static bool? Complex3(bool a) => a ? false : (bool?)null;
	public static bool? Complex4(bool a) => a ? (bool?)null : false;
	public static object Complex5(bool a) => a ? true : (object)null;
	public static object Complex6(bool a) => a ? (object)null : true;
	public static object Complex7(bool a) => a ? false : (object)null;
	public static object Complex8(bool a) => a ? (object)null : false;
}
