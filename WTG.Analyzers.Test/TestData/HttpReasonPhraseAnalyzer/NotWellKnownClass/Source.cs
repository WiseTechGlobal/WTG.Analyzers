class Foo
{
	public string ReasonPhrase { get; set; }

	public void Method()
	{
		ReasonPhrase = "foo";
		this.ReasonPhrase = "bar";
	}
}
