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
				invocation.ArgumentList.Arguments.Any(a => ExpressionLooksSuspicious(a.Expression)) &&
				IsPathCombine(context.SemanticModel, invocation, context.CancellationToken))
			{
				foreach (var argument in invocation.ArgumentList.Arguments)
				{
					if (ExpressionIsSuspicious(context.SemanticModel, argument.Expression, context.CancellationToken))
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

		static bool ExpressionLooksSuspicious(ExpressionSyntax expression)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)expression;
					var text = literal.Token.ValueText;
					return TextContainsPathSeparator(text);

				case SyntaxKind.AddExpression:
					var add = (BinaryExpressionSyntax)expression;
					return ExpressionLooksSuspicious(add.Left) || ExpressionLooksSuspicious(add.Right);

				case SyntaxKind.InterpolatedStringExpression:
					var interpolatedString = (InterpolatedStringExpressionSyntax)expression;
					return InterpolatedStringIsSuspicious(interpolatedString);

				case SyntaxKind.IdentifierName:
					return true;

				default:
					return false;
			}
		}

		static bool ExpressionIsSuspicious(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)expression;
					return LiteralExpressionIsSuspicious(literal);

				case SyntaxKind.AddExpression:
					var add = (BinaryExpressionSyntax)expression;
					return ExpressionIsSuspicious(semanticModel, add.Left, cancellationToken) || ExpressionIsSuspicious(semanticModel, add.Right, cancellationToken);

				case SyntaxKind.InterpolatedStringExpression:
					var interpolatedString = (InterpolatedStringExpressionSyntax)expression;
					return InterpolatedStringIsSuspicious(interpolatedString);

				case SyntaxKind.IdentifierName:
					return IdentifierNameIsSuspicious(semanticModel, expression, cancellationToken);

				default:
					return false;
			}
		}

		static bool LiteralExpressionIsSuspicious(LiteralExpressionSyntax syntax)
		{
			var text = syntax.Token.ValueText;
			return TextContainsPathSeparator(text);
		}

		static bool InterpolatedStringIsSuspicious(InterpolatedStringExpressionSyntax syntax)
		{
			foreach (var section in syntax.Contents)
			{
				if (!section.IsKind(SyntaxKind.InterpolatedStringText))
				{
					continue;
				}

				var text = (InterpolatedStringTextSyntax)section;
				if (TextContainsPathSeparator(text.TextToken.ValueText))
				{
					return true;
				}
			}

			return false;
		}

		static bool IdentifierNameIsSuspicious(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var constant = semanticModel.GetConstantValue(expression, cancellationToken);
			if (!constant.HasValue || !(constant.Value is string value))
			{
				return false;
			}

			return TextContainsPathSeparator(value);
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
