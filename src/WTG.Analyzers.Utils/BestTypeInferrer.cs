using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
//using Microsoft.CodeAnalysis.CSharp.BestTypeInferrer;

namespace WTG.Analyzers.Utils
{
	/* I'm unsure if this class has any general utility but, if we eventually
	 * make an analyzer to check for "useless" explicitly typed arrays then
	 * it will come in handy */
	public static class BestTypeInferrer
	{
		// An O(n) solution to finding the best element in a set where the pairwise betterness operation is
		// not necessarily transitive (S: {X, Y, Z}, X -> Y, Y -> Z, X ⌿> Z)
		static Type? GetBestType (List<Type> types)
		{
			switch (types.Count)
			{
				case 0:
					return null;
				case 1:
					return types[0];
			}

			int bestIndex = 0;
			Type? bestType = types[0];

			for (int i = 0; i < types.Count; ++i)
			{
				var better = Better(types[i], bestType);

				if (better == null)
				{
					bestType = null;
				}
				else if (better == true)
				{
					bestType = types[i];
					bestIndex = i;
				}
			}

			if (bestType == null)
			{
				return null;
			}

			// we've only checked that type at bestIndex is better than all types
			// after bestIndex so...
			for (int i = 0; i < bestIndex; ++i)
			{
				var better = Better(types[i], bestType);

				if (better == null)
				{
					return null;
				}
				else if (better == true)
				{
					if (!bestType.Equals(types[i]))
					{
						return null;
					}
				}
			}

			return bestType;
		}

		// return true if a IS BETTER OR EQUAL TO than b
		static bool? Better(Type? a, Type? b)
		{
			if (a == null)
			{
				return false;
			}

			if (b == null)
			{
				return true;
			}

			if (a == b)
			{
				return true;
			}

			var aTob = HasImplicitConversion(a, b);
			var bToa = HasImplicitConversion(b, a);

			if (aTob && bToa)
			{
				return true;
			}

			if (aTob)
			{
				return false;
			}

			if (bToa)
			{
				return true;
			}

			return null;
		}

		static readonly Dictionary<Type, List<Type>> standardImplicitNumericCasts = new Dictionary<Type, List<Type>>()
		{
			{ typeof(sbyte), new List<Type> { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(byte), new List<Type> { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(short), new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(ushort), new List<Type> { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(int), new List<Type> { typeof(long), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(uint), new List<Type> { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(long), new List<Type> { typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(char), new List<Type> { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
			{ typeof(float), new List<Type> { typeof(double) } },
			{ typeof(ulong), new List<Type> { typeof(float), typeof(double), typeof(decimal) } },
		};

		static bool HasImplicitConversion(Type source, Type dest)
		{
			if (source == null || dest == null)
			{
				return false;
			}

			if (source == dest || source.GetInterfaces().Contains(dest) || source.BaseType == dest)
			{
				return true;
			}

			if (standardImplicitNumericCasts.ContainsKey(source) && standardImplicitNumericCasts[source].Contains(dest))
			{
				return true;
			}

			return dest.IsAssignableFrom(source) || source.GetMethods(BindingFlags.Public | BindingFlags.Static)
				   .Where(method => method.Name == "op_Implicit" && method.ReturnType == dest)
				   .Any(possibleImplicitConversion =>
				   {
					   ParameterInfo parameter = possibleImplicitConversion.GetParameters().FirstOrDefault();
					   return parameter?.ParameterType == source;
				   });
		}
	}
}
