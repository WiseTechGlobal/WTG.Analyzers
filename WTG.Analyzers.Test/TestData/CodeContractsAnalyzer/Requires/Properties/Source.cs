using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public object Property1
	{
		get => field;
		set
		{
			Contract.Requires(value != null);
			field = value;
		}
	}

	public object Property2
	{
		get => field;
		private set
		{
			Contract.Requires(value != null);
			field = value;
		}
	}

	object Property3
	{
		get => field;
		set
		{
			Contract.Requires(value != null);
			field = value;
		}
	}

	object field;
}
