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
	internal event EventHandler InternalEvent;
	internal class InternalClass { }

	protected string ProtectedField;
	protected string ProtectedProperty => ProtectedField;
	protected string ProtectedMethod() => ProtectedField;
	protected event EventHandler ProtectedEvent;
	protected class ProtectedClass { }

	private protected string PrivateProtectedField;
	private protected string PrivateProtectedProperty => PrivateProtectedField;
	private protected string PrivateProtectedMethod() => PrivateProtectedField;
	private protected event EventHandler PrivateProtectedEvent;
	private protected class PrivateProtectedClass { }

	private string PrivateField;
	private string PrivateProperty => PrivateField;
	private string PrivateMethod() => PrivateField;
	private event EventHandler PrivateEvent;
	private class PrivateClass { }

	string ImplicitPrivateField;
	string ImplicitPrivateProperty => ImplicitPrivateField;
	string ImplicitPrivateMethod() => ImplicitPrivateField;
	event EventHandler ImplicitEvent;
	class ImplicitPrivateClass { }

	public string SpecialProperty { get; private set; } // special case where private is acceptable.
}
