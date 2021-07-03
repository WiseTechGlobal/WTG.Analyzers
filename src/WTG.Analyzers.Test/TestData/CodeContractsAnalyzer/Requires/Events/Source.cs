using System;
using System.Diagnostics.Contracts;

public class Bob
{
	public event EventHandler Event1
	{
		add
		{
			Contract.Requires(value != null);
			handler += value;
		}
		remove
		{
			Contract.Requires(value != null);
			handler -= value;
		}
	}

	event EventHandler Event2
	{
		add
		{
			Contract.Requires(value != null);
			handler += value;
		}
		remove
		{
			Contract.Requires(value != null);
			handler -= value;
		}
	}

	EventHandler handler;
}
