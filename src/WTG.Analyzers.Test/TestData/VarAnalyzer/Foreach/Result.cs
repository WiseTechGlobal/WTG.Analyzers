using System.Collections;
using System.Collections.Generic;

public class Bob
{
	public void GenericForeachLoop(IEnumerable<int> values)
	{
		foreach (var value in values)
		{
		}
	}

	public void NonGenericForeachLoopAsObject(IEnumerable values)
	{
		foreach (var value in values)
		{
		}
	}

	public void TypedNonGenericCollection(MagicCollection<int> values)
	{
		foreach (var value in values)
		{
		}
	}

	public void Array(int[] values)
	{
		foreach (var value in values)
		{
		}
	}

	public void NonGenericForeachLoopAsNonObject(IEnumerable values)
	{
		foreach (int value in values) // can't translate this so don't suggest it
		{
		}
	}

	public void NonStandardEnumerator(IDictionary dictionary)
	{
		foreach (DictionaryEntry item in dictionary) // can't translate this so don't suggest it
		{
		}
	}

	public void AmbigiousItemType(AmbigiousCollection values)
	{
		foreach (int value in values) // can't translate this so don't suggest it
		{
		}

		foreach (double value in values) // can't translate this so don't suggest it
		{
		}
	}

	public void MissingTypeInfo(UnknownType values)
	{
		foreach (string value in values) // we don't have sufficient information to say if this can be converted.
		{
		}
	}

	public void ImplicitNonGeneric(ImplicitNonGenericCollection values)
	{
		foreach (int value in values) // can't translate this so don't suggest it
		{
		}
	}
}

public class MagicCollection<T> : IEnumerable
{
	public MagicEnumerator GetEnumerator()
	{
		return new MagicEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct MagicEnumerator : IEnumerator
	{
		public int Current { get; set; }

		public bool MoveNext()
		{
			Current++;
			return Current < 100;
		}

		public void Reset()
		{
			Current = 0;
		}
	}
}

public class AmbigiousCollection : IEnumerable<int>, IEnumerable<double>
{
	IEnumerator<int> IEnumerable<int>.GetEnumerator() { yield break; }
	IEnumerator<double> IEnumerable<double>.GetEnumerator() { yield break; }
}

public class ImplicitNonGenericCollection : IEnumerable<int>
{
	public IEnumerator GetEnumerator() { yield break; }
	IEnumerator<int> IEnumerable<int>.GetEnumerator() { yield break; }
}
