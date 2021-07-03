using System;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class ArrayAttribute : Attribute
{
	public ArrayAttribute(object[] unused) { }
}

[Array(new object[0])]
[ArrayAttribute(new object[0])]
public class Foo
{
}
