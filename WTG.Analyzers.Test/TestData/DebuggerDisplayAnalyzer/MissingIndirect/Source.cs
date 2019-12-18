using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("Magic = {content.MissingProperty.Additional}")]
class Foo
{
	List<int> content;
}
