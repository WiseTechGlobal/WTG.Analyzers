using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class StringBuilderAppendAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontMutateAppendedStringArgumentsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(Start);
		}

		static void Start(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(c => Analyze(c, cache), SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invokeExpression = (InvocationExpressionSyntax)context.Node;

			if (!LooksLikeAppendMethod(invokeExpression) && invokeExpression.ArgumentList.Arguments.Count == 0)
			{
				return;
			}

			var firstArgument = invokeExpression.ArgumentList.Arguments[0].Expression;

			if (!LooksLikeMutation(firstArgument))
			{
				return;
			}

			if (!IsAppendMethod(context.SemanticModel, invokeExpression, firstArgument, context.CancellationToken, out var properties))
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(Rules.DontMutateAppendedStringArgumentsRule, invokeExpression.GetLocation(), properties));
		}

		static bool LooksLikeAppendMethod(InvocationExpressionSyntax invoke)
			=> invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
				&& invoke.Expression is MemberAccessExpressionSyntax member
				&& (member.Name.Identifier.Text == nameof(StringBuilder.Append) || member.Name.Identifier.Text == nameof(StringBuilder.AppendLine));

		static bool LooksLikeMutation(ExpressionSyntax expression)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.AddExpression:
				case SyntaxKind.InterpolatedStringExpression:
					return true;

				case SyntaxKind.InvocationExpression:
					var invoke = (InvocationExpressionSyntax)expression;

					if (!invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						return false;
					}

					var member = (MemberAccessExpressionSyntax)invoke.Expression;
					return member.Name.Identifier.Text == nameof(string.Format);

				default:
					return false;
			}
		}

		static bool IsAppendMethod(SemanticModel semanticModel, InvocationExpressionSyntax invokeExpression, ExpressionSyntax firstArgument, CancellationToken cancellationToken, [NotNullWhen(true)] out ImmutableDictionary<string, string>? properties)
		{
			var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invokeExpression, cancellationToken).Symbol;

			if (symbol == null || !symbol.ContainingType.IsMatch("System.Text.StringBuilder") || symbol.Parameters.Length == 0)
			{
				properties = null;
				return false;
			}

			var parameter = symbol.Parameters[0];

			if (!parameter.Type.IsMatch("System.String"))
			{
				properties = null;
				return false;
			}

			if (symbol.Name == nameof(StringBuilder.Append))
			{
				if (firstArgument.IsKind(SyntaxKind.InterpolatedStringExpression) ||
					IsStringFormat(semanticModel, firstArgument, cancellationToken))
				{
					properties = AppendFormatMode;
				}
				else
				{
					properties = AppendMode;
				}
			}
			else if (firstArgument.IsKind(SyntaxKind.InterpolatedStringExpression) ||
				IsStringFormat(semanticModel, firstArgument, cancellationToken))
			{
				properties = AppendFormatAppendLineMode;
			}
			else if (LastAppendIsString(semanticModel, firstArgument, cancellationToken))
			{
				properties = AppendLineMode;
			}
			else
			{
				properties = AppendAppendLineMode;
			}

			return true;
		}

		static bool IsStringFormat(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			if (!expression.IsKind(SyntaxKind.InvocationExpression))
			{
				return false;
			}

			var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;

			if (symbol == null || symbol.ContainingType.SpecialType != SpecialType.System_String)
			{
				return false;
			}

			return true;
		}

		static bool LastAppendIsString(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			while (expression.IsKind(SyntaxKind.AddExpression))
			{
				var binaryExpression = (BinaryExpressionSyntax)expression;
				expression = binaryExpression.Right;
			}

			var type = semanticModel.GetTypeInfo(expression, cancellationToken).Type;

			return type != null && type.SpecialType == SpecialType.System_String;
		}

		static ImmutableDictionary<string, string> CreateProperties(StringBuilderAppendMode mode)
			=> ImmutableDictionary<string, string>.Empty.Add(nameof(StringBuilderAppendMode), mode.ToString());

		static readonly ImmutableDictionary<string, string> AppendMode = CreateProperties(StringBuilderAppendMode.Append);
		static readonly ImmutableDictionary<string, string> AppendLineMode = CreateProperties(StringBuilderAppendMode.AppendLine);
		static readonly ImmutableDictionary<string, string> AppendAppendLineMode = CreateProperties(StringBuilderAppendMode.AppendAppendLine);

		static readonly ImmutableDictionary<string, string> AppendFormatMode = CreateProperties(StringBuilderAppendMode.AppendFormatMode);
		static readonly ImmutableDictionary<string, string> AppendFormatAppendLineMode = CreateProperties(StringBuilderAppendMode.AppendFormatAppendLineMode);
	}
}
