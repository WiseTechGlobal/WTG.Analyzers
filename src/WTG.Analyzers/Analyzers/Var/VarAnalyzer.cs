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
			Rules.UseOutVarWherePossibleRule,
			Rules.DeconstructWithVarRule);

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
				SyntaxKind.UsingStatement);

			context.RegisterSyntaxNodeAction(
				c => AnalyzeForEach(c, cache),
				SyntaxKind.ForEachStatement);

			context.RegisterSyntaxNodeAction(
				c => AnalyzeInvoke(c, cache),
				SyntaxKind.InvocationExpression);

			context.RegisterSyntaxNodeAction(
				c => AnalyzeAssignment(c, cache),
				SyntaxKind.SimpleAssignmentExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var candidate = Visitor.Instance.Visit(context.Node);

			if (candidate == null || candidate.ValueSource.IsKind(SyntaxKind.DefaultLiteralExpression))
			{
				return;
			}

			var model = context.SemanticModel;
			var declaredType = model.GetTypeInfo(candidate.Type, context.CancellationToken).Type;
			var expressionTypeInfo = model.GetTypeInfo(candidate.ValueSource, context.CancellationToken);
			var expressionType = expressionTypeInfo.Type;

			if (declaredType == null || expressionType == null)
			{
				return;
			}

			if (declaredType.IsReferenceType && expressionTypeInfo.Nullability.FlowState == NullableFlowState.NotNull)
			{
				// Unfortunately, `declaredTypeInfo.Nullability.Annotation` provides wildly inconsistent/inacurate information (no better than random)
				// so we need to try get the information ourselves.
				if (candidate.Type.IsKind(SyntaxKind.NullableType))
				{
					// Declared as nullable but initialized with a non-null value. It may be set to null later.
					return;
				}
			}

			if (candidate.ValueSource.IsKind(SyntaxKind.StackAllocArrayCreationExpression) && expressionType.TypeKind == TypeKind.Struct && expressionType.IsMatch(WellKnownTypeNames.Span))
			{
				return;
			}

			if (TypeEquals(expressionType, declaredType))
			{
				context.ReportDiagnostic(Rules.CreateUseVarWherePossibleDiagnostic(candidate.Type.GetLocation()));
			}
		}

		static void AnalyzeForEach(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (ForEachStatementSyntax)context.Node;

			if (node.Type.IsVar)
			{
				return;
			}

			var model = context.SemanticModel;
			var declaredType = model.GetTypeInfo(node.Type, context.CancellationToken).Type;
			var expressionType = model.GetTypeInfo(node.Expression, context.CancellationToken).Type;

			if (declaredType == null || expressionType == null)
			{
				return;
			}

			expressionType = EnumerableTypeUtils.GetElementType(expressionType);

			if (TypeEquals(expressionType, declaredType))
			{
				context.ReportDiagnostic(Rules.CreateUseVarWherePossibleDiagnostic(node.Type.GetLocation()));
			}
		}

		static void AnalyzeInvoke(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invoke = (InvocationExpressionSyntax)context.Node;
			IMethodSymbol? knownMethod = null;

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
						knownMethod = (IMethodSymbol?)context.SemanticModel.GetSymbolInfo(invoke).Symbol;

						if (knownMethod == null)
						{
							// If we can't resolve the symbol with an explicit type, then 'var' is dead in the water.
							return;
						}
					}

					var proposedSyntax = invoke.ReplaceNode(type, SyntaxFactory.IdentifierName("var").WithTriviaFrom(type));
					var symbol = context.SemanticModel.GetSpeculativeSymbolInfo(invoke.SpanStart, proposedSyntax, SpeculativeBindingOption.BindAsExpression).Symbol;

					if (SymbolEqualityComparer.Default.Equals(knownMethod, symbol))
					{
						// We got the same symbol when using var, so var must be safe.
						context.ReportDiagnostic(Rules.CreateUseOutVarWherePossibleDiagnostic(type.GetLocation()));
					}
				}
			}
		}

		static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (AssignmentExpressionSyntax)context.Node;

			if (node.Left.IsKind(SyntaxKind.TupleExpression))
			{
				var type = context.SemanticModel.GetTypeInfo(node.Right).Type;

				if (type != null && type.IsTupleType)
				{
					CheckTupleTypes(ref context, (TupleExpressionSyntax)node.Left, (INamedTypeSymbol)type);
				}
			}
		}

		static void CheckTupleTypes(ref SyntaxNodeAnalysisContext context, TupleExpressionSyntax tupleExp, INamedTypeSymbol type)
		{
			var arguments = tupleExp.Arguments;
			var elements = type.TupleElements;

			if (elements == null)
			{
				return;
			}

			var count = Math.Min(arguments.Count, elements.Length);

			for (var i = 0; i < count; i++)
			{
				var arg = arguments[i].Expression;

				switch (arg.Kind())
				{
					case SyntaxKind.TupleExpression:
						{
							var argType = elements[i].Type;

							if (argType.Kind == SymbolKind.NamedType)
							{
								CheckTupleTypes(
									ref context,
									(TupleExpressionSyntax)arg,
									(INamedTypeSymbol)argType);
							}
						}
						break;

					case SyntaxKind.DeclarationExpression:
						{
							var declExp = (DeclarationExpressionSyntax)arg;

							if (!declExp.Type.IsVar)
							{
								var expectedType = elements[i].Type;
								var actualType = context.SemanticModel.GetTypeInfo(declExp.Type, context.CancellationToken).Type;

								if (TypeEquals(actualType, expectedType))
								{
									context.ReportDiagnostic(
										Diagnostic.Create(
											Rules.DeconstructWithVarRule,
											declExp.Type.GetLocation()));
								}
							}
						}
						break;
				}
			}
		}

		static bool TypeEquals(ITypeSymbol? x, ITypeSymbol? y) => ReferenceEquals(x, y) || (x != null && SymbolEqualityComparer.Default.Equals(x, y));

		sealed class Visitor : CSharpSyntaxVisitor<Candidate?>
		{
			public static Visitor Instance { get; } = new Visitor();

			public override Candidate? VisitForStatement(ForStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
			{
				if (node.IsConst)
				{
					return null;
				}

				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate? VisitUsingStatement(UsingStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			static Candidate? ExtractFromVariableDecl(VariableDeclarationSyntax? decl)
			{
				if (decl != null && !decl.Type.IsVar && decl.Variables.Count == 1)
				{
					var exp = decl.Variables[0].Initializer?.Value;

					if (exp != null)
					{
						return new Candidate(decl.Type, exp);
					}
				}

				return null;
			}
		}

		sealed class Candidate
		{
			public Candidate(TypeSyntax type, ExpressionSyntax valueSource)
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
			}

			public TypeSyntax Type { get; }
			public ExpressionSyntax ValueSource { get; }
		}
	}
}
