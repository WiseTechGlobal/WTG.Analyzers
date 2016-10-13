using System;

public class Bob
{
	public string PublicField;
	public string PublicProperty => PublicField;
	public string PublicMethod() => PublicField;
	public event EventHandler PublicEvent;
	public class PublicClass { }

	internal string InternalField;
	internal string InternalProperty => InternalField;
	internal string InternalMethod() => InternalField;
	internal event EventHandler PublicEvent;
	internal class InternalClass { }

	protected string ProtectedField;
	protected string ProtectedProperty => ProtectedField;
	protected string ProtectedMethod() => ProtectedField;
	protected event EventHandler PublicEvent;
	protected class ProtectedClass { }

	private string PrivateField;
	private string PrivateProperty => PrivateField;
	private string PrivateMethod() => PrivateField;
	private event EventHandler PublicEvent;
	private class PrivateClass { }

	string ImplicitPrivateField;
	string ImplicitPrivateProperty => ImplicitPrivateField;
	string ImplicitPrivateMethod() => ImplicitPrivateField;
	event EventHandler PublicEvent;
	class ImplicitPrivateClass { }

	public string SpecialProperty { get; private set; } // special case where private is acceptable.
}
