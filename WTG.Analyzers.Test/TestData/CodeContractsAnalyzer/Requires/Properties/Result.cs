using System;

public class Bob
{
	public object Property1
	{
		get => field;
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			field = value;
		}
	}

	public object Property2
	{
		get => field;
		private set
		{
			field = value;
		}
	}

	object Property3
	{
		get => field;
		set
		{
			field = value;
		}
	}

	object field;
}
