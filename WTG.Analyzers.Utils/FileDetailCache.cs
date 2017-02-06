using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	public sealed class FileDetailCache
	{
		public bool IsGenerated(SyntaxTree tree, CancellationToken token)
		{
			bool result;

			lock (cache)
			{
				if (!cache.TryGetValue(tree.FilePath, out result))
				{
					cache.Add(tree.FilePath, result = tree.IsGenerated(token));
				}
			}

			return result;
		}

		readonly Dictionary<string, bool> cache = new Dictionary<string, bool>();
	}
}
