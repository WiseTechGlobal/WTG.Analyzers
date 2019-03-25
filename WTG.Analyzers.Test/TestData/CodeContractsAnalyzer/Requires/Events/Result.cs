using System;

public class Bob
{
	public event EventHandler Event1
	{
		add
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			handler += value;
		}
		remove
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			handler -= value;
		}
	}

	event EventHandler Event2
	{
		add
		{
			handler += value;
		}
		remove
		{
			handler -= value;
		}
	}

	EventHandler handler;
}
