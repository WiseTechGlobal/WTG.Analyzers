using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class ToStringAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DontCallToStringOnAStringRule,
			Rules.PreferNameofOverCallingToStringOnAnEnumRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
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

			var invoke = (InvocationExpressionSyntax)context.Node;
			var supportsNameof = SupportsNameof(context.SemanticModel.SyntaxTree);

			switch (invoke.Expression.Kind())
			{
				case SyntaxKind.SimpleMemberAccessExpression:
					{
						var member = (MemberAccessExpressionSyntax)invoke.Expression;

						if (member.Name.Identifier.Text == WellKnownMemberNames.ObjectToString)
						{
							var symbol = (IMethodSymbol?)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

							if (symbol != null && symbol.ReceiverType != null)
							{
								switch (symbol.ReceiverType.SpecialType)
								{
									case SpecialType.System_String:
										context.ReportDiagnostic(Rules.CreateDontCallToStringOnAStringDiagnostic(InvokeLocation(invoke, member.OperatorToken)));
										break;

									case SpecialType.System_Enum:
										if (supportsNameof &&
											symbol.Parameters.Length == 0 &&
											IsEnumLiteral(member.Expression, context.SemanticModel, context.CancellationToken))
										{
											context.ReportDiagnostic(Rules.CreatePreferNameofOverCallingToStringOnAnEnumDiagnostic(invoke.GetLocation()));
										}
										break;
								}
							}
						}
					}
					break;

				case SyntaxKind.MemberBindingExpression:
					{
						var binding = (MemberBindingExpressionSyntax)invoke.Expression;

						if (binding.Name.Identifier.Text == WellKnownMemberNames.ObjectToString)
						{
							var symbol = (IMethodSymbol?)context.SemanticModel.GetSymbolInfo(invoke, context.CancellationToken).Symbol;

							if (symbol?.ReceiverType?.SpecialType == SpecialType.System_String)
							{
								context.ReportDiagnostic(Rules.CreateDontCallToStringOnAStringDiagnostic(InvokeLocation(invoke, binding.OperatorToken)));
							}
						}
					}
					break;
			}
		}

		static bool SupportsNameof(SyntaxTree syntaxTree)
		{
			var languageVersion = ((CSharpParseOptions)syntaxTree.Options).LanguageVersion;

			return languageVersion >= LanguageVersion.CSharp6;
		}

		static bool IsEnumLiteral(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
		{
			while (expression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				var paren = (ParenthesizedExpressionSyntax)expression;
				expression = paren.Expression;
			}

			var memberSymbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;

			return memberSymbol != null &&
				memberSymbol.Kind == SymbolKind.Field &&
				memberSymbol.IsStatic &&
				memberSymbol.ContainingType.EnumUnderlyingType != null;
		}

		static Location InvokeLocation(InvocationExpressionSyntax invoke, SyntaxToken operatorToken)
		{
			//   -->...........<--
			// value.ToString()
			return Location.Create(
				invoke.SyntaxTree,
				TextSpan.FromBounds(
					operatorToken.Span.Start,
					invoke.Span.End));
		}
	}
}
