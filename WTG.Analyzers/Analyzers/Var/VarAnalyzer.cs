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
			Rules.UseVarWherePossibleRule);

		public override void Initialize(AnalysisContext context)
		{
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

					if (expressionType == typeSymbol)
					{
						context.ReportDiagnostic(Rules.CreateUseVarWherePossibleDiagnostic(candidate.Type.GetLocation()));
					}
				}
			}
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
				if (type == null) throw new ArgumentNullException(nameof(type));
				if (valueSource == null) throw new ArgumentNullException(nameof(valueSource));

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
