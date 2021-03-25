using System.Collections.Immutable;
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

			if (!IsAppendMethod(context.SemanticModel, invokeExpression, context.CancellationToken) ||
				!IsMutation(context.SemanticModel, firstArgument, context.CancellationToken))
			{
				return;
			}

			context.ReportDiagnostic(Rules.CreateDontMutateAppendedStringArgumentsDiagnostic(invokeExpression.GetLocation()));
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
					var name = member.Name.Identifier.Text;
					return name == nameof(string.Format)
						|| name == nameof(string.Substring);

				default:
					return false;
			}
		}

		static bool IsAppendMethod(SemanticModel semanticModel, InvocationExpressionSyntax invokeExpression, CancellationToken cancellationToken)
		{
			var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invokeExpression, cancellationToken).Symbol;

			if (symbol == null || !symbol.ContainingType.IsMatch("System.Text.StringBuilder") || symbol.Parameters.Length == 0)
			{
				return false;
			}

			var parameter = symbol.Parameters[0];

			if (parameter.Type.SpecialType != SpecialType.System_String)
			{
				return false;
			}

			return true;
		}

		static bool IsMutation(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.InterpolatedStringExpression:
				case SyntaxKind.AddExpression:
					return true;

				case SyntaxKind.InvocationExpression:
					var symbol = (IMethodSymbol)semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;

					return symbol != null
						&& symbol.ContainingType.SpecialType == SpecialType.System_String
						&& (symbol.Name == nameof(string.Format) || symbol.Name == nameof(string.Substring));

				default:
					return false;
			}
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
	}
}
