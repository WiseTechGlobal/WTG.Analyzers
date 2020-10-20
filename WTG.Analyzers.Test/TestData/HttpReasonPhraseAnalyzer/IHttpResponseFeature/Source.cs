using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Features
{
	public interface IHttpResponseFeature
    {
		string ReasonPhrase { get; set; }
    }
}

class Foo
{
	public void Method(IHttpResponseFeature feature)
	{
		feature.ReasonPhrase = "foo";
		Log(feature.ReasonPhrase);
	}

	void Log(string value)
	{
	}
}
