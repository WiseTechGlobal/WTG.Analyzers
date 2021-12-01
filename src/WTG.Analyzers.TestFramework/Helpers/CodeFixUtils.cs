using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace WTG.Analyzers.TestFramework
{
	public static class CodeFixUtils
	{
		public static async Task CollectCodeActions(CodeFixProvider provider, Document document, Diagnostic diagnostic, ICollection<Tuple<Diagnostic, CodeAction>> actions)
		{
			if (actions == null)
			{
				throw new ArgumentNullException(nameof(actions));
			}

			var context = new CodeFixContext(document, diagnostic, (a, b) => actions.Add(Tuple.Create(diagnostic, a)), CancellationToken.None);
			await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);
		}
	}
}
