using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Analyzers.BooleanLiteral;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class FileSystemPathsAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotUsePathSeparatorsInPathLiteralsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp4))
			{
				return;
			}

			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invocation = (InvocationExpressionSyntax)context.Node;

			if (LooksLikePathCombine(invocation) &&
				invocation.ArgumentList.Arguments.Any(a => InvocationArgumentLooksSuspicious(a)) &&
				IsPathCombine(context.SemanticModel, invocation, context.CancellationToken))
			{
				foreach (var argument in invocation.ArgumentList.Arguments)
				{
					if (InvocationArgumentIsSuspicious(context.SemanticModel, argument, context.CancellationToken))
					{
						context.ReportDiagnostic(Diagnostic.Create(
							Rules.DoNotUsePathSeparatorsInPathLiteralsRule,
							argument.GetLocation()));
					}
				}
			}
		}

		static bool LooksLikePathCombine(InvocationExpressionSyntax syntax)
		{
			if (syntax.Expression is null || !syntax.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				return false;
			}

			var smae = (MemberAccessExpressionSyntax)syntax.Expression;
			if (smae.Name.Identifier.Text != nameof(Path.Combine))
			{
				return false;
			}

			var arguments = syntax.ArgumentList;
			return syntax.ArgumentList.Arguments.Count >= 2;
		}

		static bool InvocationArgumentLooksSuspicious(ArgumentSyntax argument)
		{
			switch (argument.Expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)argument.Expression;
					var text = literal.Token.ValueText;
					return TextContainsPathSeparator(text);

				case SyntaxKind.AddExpression:
				case SyntaxKind.InterpolatedStringExpression:
					// TODO
					return true;

				case SyntaxKind.IdentifierName:
					return true;

				default:
					return false;
			}
		}

		static bool InvocationArgumentIsSuspicious(SemanticModel semanticModel, ArgumentSyntax argument, CancellationToken cancellationToken)
		{
			switch (argument.Expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)argument.Expression;
					var text = literal.Token.ValueText;
					return TextContainsPathSeparator(text);

				case SyntaxKind.AddExpression:
				case SyntaxKind.InterpolatedStringExpression:
					// TODO
					return true;

				case SyntaxKind.IdentifierName:
					var constant = semanticModel.GetConstantValue(argument.Expression, cancellationToken);
					if (constant.HasValue && constant.Value is string value)
					{
						return TextContainsPathSeparator(value);
					}
					return true;

				default:
					return false;
			}
		}

		static bool TextContainsPathSeparator(string text)
		{
			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '/' || text[i] == '\\')
				{
					return true;
				}
			}

			return false;
		}

		static bool IsPathCombine(SemanticModel semanticModel, InvocationExpressionSyntax syntax, CancellationToken cancellationToken)
		{
			var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(syntax, cancellationToken).Symbol;
			if (symbol is null || !symbol.ContainingType.IsMatch("System.IO.Path") || symbol.Parameters.Length == 0)
			{
				return false;
			}

			if (symbol.Parameters.Length == 1)
			{
				var parameter = symbol.Parameters[0];
				if (parameter.Type.SpecialType != SpecialType.System_Array)
				{
					return false;
				}
			}
			else
			{
				foreach (var parameter in symbol.Parameters)
				{
					if (parameter.Type.SpecialType != SpecialType.System_String)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
