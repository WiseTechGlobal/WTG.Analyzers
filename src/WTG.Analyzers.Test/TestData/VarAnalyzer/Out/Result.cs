using System;
using System.Collections.Generic;

public class Bob
{
	public void V1() => Unambiguous(out var value);
	public void V2() => Generic<int>(out var value);
	public void V3() => Generic(1, out var value);
	public void V4() => Generic(TryGet("key", out var value1), out var value2);
	public void V5() => Lookup.TryGetValue("key", out var value);

	public void S4() => SemiAmbiguous(out var value1, out int value2, out var value3); // value1 and value2 cannot both be var as that would make it ambiguous.

	public void X1() => Ambiguous(out int value); // the type is needed to disambiguate the overload.
	public void X2() => Generic(out int value); // the type is needed to disambiguate the type argument.
	public void X3() => Generic(5, out double value); // using var would cause a different type to be inferred.
	public void X4() => Unambiguous(out var value); // already using var.

	public void N()
	{
		int value;
		Unambiguous(out value); // type not specified.
	}

	void Unambiguous(out int value) => value = 0;
	void Ambiguous(out int value) => value = 0;
	void Ambiguous(out double value) => value = 0;
	void SemiAmbiguous(out int value1, out int value2, out bool value3) => value3 = (value1 = value2 = 0) == 0;
	void SemiAmbiguous(out double value1, out double value2, out bool value3) => value3 = (value1 = value2 = 0) == 0;
	void Generic<T>(out T value) => value = default(T);
	void Generic<T>(T inValue, out T outValue) => outValue = inValue;
	bool TryGet(string key, out string value) => (value = key) != null;
	Dictionary<string, string> Lookup { get; } = new Dictionary<string, string>();
}
