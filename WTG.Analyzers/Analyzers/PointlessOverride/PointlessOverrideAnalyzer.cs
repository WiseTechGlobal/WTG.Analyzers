using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	class PointlessOverrideAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.RemovePointlessOverridesRule);

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
				SyntaxKind.MethodDeclaration,
				SyntaxKind.PropertyDeclaration,
				SyntaxKind.IndexerDeclaration,
				SyntaxKind.EventDeclaration);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var node = (CSharpSyntaxNode)context.Node;

			if (IsOverride(node) && !node.Accept(IsMeaningfulVisitor.Instance))
			{
				context.ReportDiagnostic(CreateDiagnostic(node));
			}
		}

		static Diagnostic CreateDiagnostic(CSharpSyntaxNode node)
		{
			switch (node.Kind())
			{
				case SyntaxKind.PropertyDeclaration:
					return Rules.CreateRemovePointlessOverrides_PropertyDiagnostic(node.GetLocation());

				case SyntaxKind.MethodDeclaration:
					return Rules.CreateRemovePointlessOverrides_MethodDiagnostic(node.GetLocation());

				case SyntaxKind.IndexerDeclaration:
					return Rules.CreateRemovePointlessOverrides_IndexerDiagnostic(node.GetLocation());

				case SyntaxKind.EventDeclaration:
					return Rules.CreateRemovePointlessOverrides_EventDiagnostic(node.GetLocation());

				default:
					// shouldn't happen, but just in case.
					return Rules.CreateRemovePointlessOverridesDiagnostic(node.GetLocation());
			}
		}

		static bool IsOverride(CSharpSyntaxNode node)
		{
			var modifiers = node.Accept(ModifierExtractionVisitor.Instance);

			foreach (var modifier in modifiers)
			{
				if (modifier.IsKind(SyntaxKind.OverrideKeyword))
				{
					return true;
				}
			}

			return false;
		}

		static bool IsBaseMemberAccess(ExpressionSyntax node)
			=> node != null
				&& node.IsKind(SyntaxKind.SimpleMemberAccessExpression)
				&& IsBaseMemberAccess((MemberAccessExpressionSyntax)node);

		static bool IsBaseMemberAccess(MemberAccessExpressionSyntax node)
			=> node.Expression.IsKind(SyntaxKind.BaseExpression);

		sealed class IsMeaningfulVisitor : CSharpSyntaxVisitor<bool>
		{
			public static IsMeaningfulVisitor Instance { get; } = new IsMeaningfulVisitor();

			IsMeaningfulVisitor()
			{
			}

			public override bool DefaultVisit(SyntaxNode node) => true;

			public override bool VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				var expression = node.Accept(SolitaryExpressionLocator.Instance);

				if (expression == null || !expression.IsKind(SyntaxKind.InvocationExpression))
				{
					return true;
				}

				var invoke = (InvocationExpressionSyntax)expression;

				if (!invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
				{
					return true;
				}

				var member = (MemberAccessExpressionSyntax)invoke.Expression;

				return !IsBaseMemberAccess(member)
					|| !IsMatch(node, member)
					|| !IsMatch(node.ParameterList, invoke.ArgumentList);
			}

			public override bool VisitPropertyDeclaration(PropertyDeclarationSyntax node)
			{
				if (node.ExpressionBody != null)
				{
					return !IsMatchingSelf(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.GetAccessorDeclaration:
							{
								var expression = accessor.Accept(SolitaryExpressionLocator.Instance);

								if (!IsMatchingSelf(expression))
								{
									return true;
								}
							}
							break;

						case SyntaxKind.SetAccessorDeclaration:
							{
								var expression = GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance));

								if (!IsMatchingSelf(expression))
								{
									return true;
								}
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsMatchingSelf(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						return false;
					}

					var propertyRef = (MemberAccessExpressionSyntax)expression;

					if (!IsBaseMemberAccess(propertyRef) || !IsMatch(node, propertyRef))
					{
						return false;
					}

					return true;
				}
			}

			public override bool VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
				if (node.ExpressionBody != null)
				{
					return !IsMatchingSelf(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.GetAccessorDeclaration:
							if (!IsMatchingSelf(accessor.Accept(SolitaryExpressionLocator.Instance)))
							{
								return true;
							}
							break;

						case SyntaxKind.SetAccessorDeclaration:
							if (!IsMatchingSelf(GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance))))
							{
								return true;
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsMatchingSelf(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.ElementAccessExpression))
					{
						return false;
					}

					var element = (ElementAccessExpressionSyntax)expression;

					if (!element.Expression.IsKind(SyntaxKind.BaseExpression) ||
						!IsMatch(node.ParameterList, element.ArgumentList))
					{
						return false;
					}

					return true;
				}
			}

			public override bool VisitEventDeclaration(EventDeclarationSyntax node)
			{
				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.AddAccessorDeclaration:
							if (!IsMatchingSelf(GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.AddAssignmentExpression)))
							{
								return true;
							}
							break;

						case SyntaxKind.RemoveAccessorDeclaration:
							if (!IsMatchingSelf(GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.SubtractAssignmentExpression)))
							{
								return true;
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsMatchingSelf(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						return false;
					}

					var propertyRef = (MemberAccessExpressionSyntax)expression;

					if (!IsBaseMemberAccess(propertyRef) || !IsMatch(node, propertyRef))
					{
						return false;
					}

					return true;
				}
			}

			static ExpressionSyntax GetAssignmentTarget(ExpressionSyntax expression, SyntaxKind kind = SyntaxKind.SimpleAssignmentExpression)
			{
				if (expression.IsKind(kind))
				{
					var assignment = (AssignmentExpressionSyntax)expression;

					if (assignment.Right.IsKind(SyntaxKind.IdentifierName))
					{
						var identifier = ((IdentifierNameSyntax)assignment.Right);

						if (identifier.Identifier.Text == "value")
						{
							return assignment.Left;
						}
					}
				}

				return null;
			}

			static bool IsMatch(ParameterListSyntax parameterList, ArgumentListSyntax argumentList)
				=> IsMatch(parameterList.Parameters, argumentList.Arguments);

			static bool IsMatch(BracketedParameterListSyntax parameterList, BracketedArgumentListSyntax argumentList)
				=> IsMatch(parameterList.Parameters, argumentList.Arguments);

			static bool IsMatch(SeparatedSyntaxList<ParameterSyntax> parameters, SeparatedSyntaxList<ArgumentSyntax> arguments)
			{
				if (parameters.Count != arguments.Count)
				{
					return false;
				}

				for (var i = 0; i < parameters.Count; i++)
				{
					if (!IsMatch(parameters[i], arguments[i]))
					{
						return false;
					}
				}

				return true;
			}

			static bool IsMatch(ParameterSyntax parameter, ArgumentSyntax argument)
			{
				if (argument.NameColon != null ||
					argument.Expression.Kind() != SyntaxKind.IdentifierName)
				{
					return false;
				}

				var identifier = (IdentifierNameSyntax)argument.Expression;

				if (parameter.Identifier.Text != identifier.Identifier.Text)
				{
					return false;
				}

				return true;
			}

			static bool IsMatch(MethodDeclarationSyntax declaration, MemberAccessExpressionSyntax expression)
				=> declaration.Identifier.Text == expression.Name.Identifier.Text;

			static bool IsMatch(PropertyDeclarationSyntax declaration, MemberAccessExpressionSyntax expression)
				=> declaration.Identifier.Text == expression.Name.Identifier.Text;

			static bool IsMatch(EventDeclarationSyntax declaration, MemberAccessExpressionSyntax expression)
				=> declaration.Identifier.Text == expression.Name.Identifier.Text;
		}

		sealed class SolitaryExpressionLocator : CSharpSyntaxVisitor<ExpressionSyntax>
		{
			public static SolitaryExpressionLocator Instance { get; } = new SolitaryExpressionLocator();

			SolitaryExpressionLocator()
			{
			}

			public override ExpressionSyntax DefaultVisit(SyntaxNode node) => null;

			public override ExpressionSyntax VisitMethodDeclaration(MethodDeclarationSyntax node)
				=> node.ExpressionBody?.Accept(this) ?? node.Body?.Accept(this);

			public override ExpressionSyntax VisitAccessorDeclaration(AccessorDeclarationSyntax node)
				=> node.ExpressionBody?.Accept(this) ?? node.Body?.Accept(this);

			public override ExpressionSyntax VisitBlock(BlockSyntax node)
			{
				if (node.Statements.Count == 1)
				{
					return node.Statements[0].Accept(this);
				}

				return null;
			}

			public override ExpressionSyntax VisitArrowExpressionClause(ArrowExpressionClauseSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitExpressionStatement(ExpressionStatementSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitReturnStatement(ReturnStatementSyntax node) => node.Expression?.Accept(this);

			public override ExpressionSyntax VisitAssignmentExpression(AssignmentExpressionSyntax node) => node;
			public override ExpressionSyntax VisitElementAccessExpression(ElementAccessExpressionSyntax node) => node;
			public override ExpressionSyntax VisitInvocationExpression(InvocationExpressionSyntax node) => node;
			public override ExpressionSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => node;
		}
	}
}
