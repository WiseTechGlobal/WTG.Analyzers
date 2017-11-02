using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class VarAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UseVarWherePossibleRule,
			Rules.UseOutVarWherePossibleRule);

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
				SyntaxKind.LocalDeclarationStatement,
				SyntaxKind.ForStatement,
				SyntaxKind.ForEachStatement,
				SyntaxKind.UsingStatement);

			context.RegisterSyntaxNodeAction(
				c => AnalyzeInvoke(c, cache),
				SyntaxKind.InvocationExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var candidate = Visitor.Instance.Visit(context.Node);

			if (candidate != null)
			{
				var model = context.SemanticModel;
				var typeSymbol = (ITypeSymbol)model.GetSymbolInfo(candidate.Type, context.CancellationToken).Symbol;
				var expressionType = model.GetTypeInfo(candidate.ValueSource, context.CancellationToken).Type;

				if (typeSymbol != null && expressionType != null)
				{
					if (candidate.Unwrap)
					{
						expressionType = EnumerableTypeUtils.GetElementType(expressionType);

						if (typeSymbol == null)
						{
							return;
						}
					}

					if (TypeEquals(expressionType, typeSymbol))
					{
						context.ReportDiagnostic(Rules.CreateUseVarWherePossibleDiagnostic(candidate.Type.GetLocation()));
					}
				}
			}
		}

		static void AnalyzeInvoke(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)context.Node;
			IMethodSymbol knownMethod = null;

			foreach (var arg in invoke.ArgumentList.Arguments)
			{
				if (arg.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
				{
					var expression = arg.Expression;

					if (expression == null || !expression.IsKind(SyntaxKind.DeclarationExpression))
					{
						continue;
					}

					var type = ((DeclarationExpressionSyntax)expression).Type;

					if (type == null || type.IsVar)
					{
						continue;
					}

					if (knownMethod == null)
					{
						knownMethod = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invoke).Symbol;

						if (knownMethod == null)
						{
							// If we can't resolve the symbol with an explicit type, then 'var' is dead in the water.
							return;
						}
					}

					var proposedSyntax = invoke.ReplaceNode(type, SyntaxFactory.IdentifierName("var").WithTriviaFrom(type));
					var symbol = context.SemanticModel.GetSpeculativeSymbolInfo(invoke.SpanStart, proposedSyntax, SpeculativeBindingOption.BindAsExpression).Symbol;

					if (knownMethod.Equals(symbol))
					{
						// We got the same symbol when using var, so var must be safe.
						context.ReportDiagnostic(Rules.CreateUseOutVarWherePossibleDiagnostic(type.GetLocation()));
					}
				}
			}
		}

		static bool TypeEquals(ITypeSymbol x, ITypeSymbol y)
		{
			return x == y || (x != null && x.Equals(y));
		}

		sealed class Visitor : CSharpSyntaxVisitor<Candidate>
		{
			public static Visitor Instance { get; } = new Visitor();

			public override Candidate VisitForEachStatement(ForEachStatementSyntax node)
			{
				if (!node.Type.IsVar)
				{
					return new Candidate(node.Type, node.Expression, true);
				}

				return null;
			}

			public override Candidate VisitForStatement(ForStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
			{
				if (node.IsConst)
				{
					return null;
				}

				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate VisitUsingStatement(UsingStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			static Candidate ExtractFromVariableDecl(VariableDeclarationSyntax decl)
			{
				if (decl != null && !decl.Type.IsVar && decl.Variables.Count == 1)
				{
					var exp = decl.Variables[0].Initializer?.Value;

					if (exp != null)
					{
						return new Candidate(decl.Type, exp, false);
					}
				}

				return null;
			}
		}

		sealed class Candidate
		{
			public Candidate(TypeSyntax type, ExpressionSyntax valueSource, bool unwrap)
			{
				if (type == null)
				{
					throw new ArgumentNullException(nameof(type));
				}

				if (valueSource == null)
				{
					throw new ArgumentNullException(nameof(valueSource));
				}

				Type = type;
				ValueSource = valueSource;
				Unwrap = unwrap;
			}

			public TypeSyntax Type { get; }
			public ExpressionSyntax ValueSource { get; }
			public bool Unwrap { get; }
		}
	}
}
