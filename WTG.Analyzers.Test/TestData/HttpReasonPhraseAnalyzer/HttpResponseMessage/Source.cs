using System.Net.Http;

namespace System.Net.Http
{
	public class HttpResponseMessage
	{
		public string ReasonPhrase { get; set; }
	}
}

class Foo
{
	public void Method(HttpResponseMessage response)
	{
		response.ReasonPhrase = "foo";
	}
}
