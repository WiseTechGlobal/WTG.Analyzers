using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class RegexAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rules.ForbidCompiledInStaticRegexMethodsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(StartCompilation);
		}

		static void StartCompilation(CompilationStartAnalysisContext context)
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

			var invoke = (InvocationExpressionSyntax)context.Node;

			if (IsTargetMethod(context.SemanticModel, invoke, out var index, context.CancellationToken))
			{
				var argument = invoke.ArgumentList.Arguments[index];
				var location = new FlagLocator(context.SemanticModel, context.CancellationToken).Visit(argument);

				if (location != null)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Rules.ForbidCompiledInStaticRegexMethodsRule,
							location));
				}
			}
		}

		static bool IsTargetMethod(SemanticModel model, InvocationExpressionSyntax invoke, out int index, CancellationToken cancellationToken)
		{
			var name = ExpressionHelper.GetMethodName(invoke)?.Identifier.Text;

			switch (name)
			{
				case nameof(Regex.IsMatch):
				case nameof(Regex.Match):
				case nameof(Regex.Matches):
				case nameof(Regex.Replace):
				case nameof(Regex.Split):
					break;

				default:
					index = 0;
					return false;
			}

			var symbol = model.GetSymbolInfo(invoke, cancellationToken).Symbol;

			if (symbol == null ||
				symbol.Kind != SymbolKind.Method ||
				!symbol.IsStatic ||
				!symbol.ContainingType.IsMatch("System.Text.RegularExpressions.Regex"))
			{
				index = 0;
				return false;
			}

			var methodSymbol = (IMethodSymbol)symbol;
			var parameters = methodSymbol.Parameters;

			for (var i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].Type.IsMatch("System.Text.RegularExpressions.RegexOptions"))
				{
					index = i;
					return true;
				}
			}

			index = 0;
			return false;
		}

		sealed class FlagLocator : CSharpSyntaxVisitor<Location?>
		{
			public FlagLocator(SemanticModel semanticModel, CancellationToken cancellationToken)
			{
				this.semanticModel = semanticModel;
				this.cancellationToken = cancellationToken;
			}

			public override Location? DefaultVisit(SyntaxNode node) => null;
			public override Location? VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression.Accept(this);
			public override Location? VisitArgument(ArgumentSyntax node) => node.Expression.Accept(this);

			public override Location? VisitBinaryExpression(BinaryExpressionSyntax node)
			{
				switch (node.Kind())
				{
					case SyntaxKind.BitwiseAndExpression:
					case SyntaxKind.BitwiseOrExpression:
						return node.Left.Accept(this) ?? node.Right.Accept(this);

					default:
						return null;
				}
			}

			public override Location? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
				if (node.Name.Identifier.Text == nameof(RegexOptions.Compiled))
				{
					var symbol = semanticModel.GetSymbolInfo(node, cancellationToken).Symbol;

					if (symbol != null &&
						symbol.Kind == SymbolKind.Field &&
						symbol.ContainingType.IsMatch("System.Text.RegularExpressions.RegexOptions"))
					{
						return node.GetLocation();
					}
				}

				return null;
			}

			readonly SemanticModel semanticModel;
			readonly CancellationToken cancellationToken;
		}
	}
}
