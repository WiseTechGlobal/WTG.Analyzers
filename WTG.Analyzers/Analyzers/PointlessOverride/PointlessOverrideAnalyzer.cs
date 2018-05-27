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
				context.ReportDiagnostic(Rules.CreateRemovePointlessOverridesDiagnostic(node.GetLocation()));
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
			=> node.Expression != null && node.Expression.IsKind(SyntaxKind.BaseExpression);

		static bool IsValue(ExpressionSyntax node)
			=> node != null
				&& node.IsKind(SyntaxKind.IdentifierName)
				&& IsValue((IdentifierNameSyntax)node);

		static bool IsValue(IdentifierNameSyntax node)
			=> node.Identifier.Text == "value";

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
					return IsGetMeaningful(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.GetAccessorDeclaration:
							var expression = accessor.Accept(SolitaryExpressionLocator.Instance);

							if (IsGetMeaningful(expression))
							{
								return true;
							}
							break;

						case SyntaxKind.SetAccessorDeclaration:
							if (IsSetMeaningful(accessor.Accept(SolitaryExpressionLocator.Instance)))
							{
								return true;
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsGetMeaningful(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						return true;
					}

					var propertyRef = (MemberAccessExpressionSyntax)expression;

					if (!IsBaseMemberAccess(propertyRef) || !IsMatch(node, propertyRef))
					{
						return true;
					}

					return false;
				}

				bool IsSetMeaningful(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
					{
						return true;
					}

					var assignment = (AssignmentExpressionSyntax)expression;

					if (!IsValue(assignment.Right))
					{
						return true;
					}

					var propertyRef = (MemberAccessExpressionSyntax)assignment.Left;

					if (!IsBaseMemberAccess(assignment.Left) || !IsMatch(node, propertyRef))
					{
						return true;
					}

					return false;
				}
			}

			public override bool VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
				if (node.ExpressionBody != null)
				{
					return IsGetMeaningful(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.GetAccessorDeclaration:
							if (IsGetMeaningful(accessor.Accept(SolitaryExpressionLocator.Instance)))
							{
								return true;
							}
							break;

						case SyntaxKind.SetAccessorDeclaration:
							if (IsSetMeaningful(accessor.Accept(SolitaryExpressionLocator.Instance)))
							{
								return true;
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsGetMeaningful(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.ElementAccessExpression))
					{
						return true;
					}

					var element = (ElementAccessExpressionSyntax)expression;

					if (!element.Expression.IsKind(SyntaxKind.BaseExpression) ||
						!IsMatch(node.ParameterList, element.ArgumentList))
					{
						return true;
					}

					return false;
				}

				bool IsSetMeaningful(ExpressionSyntax expression)
				{
					if (!expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
					{
						return true;
					}

					var assignment = (AssignmentExpressionSyntax)expression;

					if (!assignment.Left.IsKind(SyntaxKind.ElementAccessExpression) || !IsValue(assignment.Right))
					{
						return true;
					}

					var element = (ElementAccessExpressionSyntax)assignment.Left;

					if (!element.Expression.IsKind(SyntaxKind.BaseExpression) || !IsMatch(node.ParameterList, element.ArgumentList))
					{
						return true;
					}

					return false;
				}
			}

			public override bool VisitEventDeclaration(EventDeclarationSyntax node)
			{
				foreach (var accessor in node.AccessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.AddAccessorDeclaration:
							if (IsAssignmentMeaningful(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.AddAssignmentExpression))
							{
								return true;
							}
							break;

						case SyntaxKind.RemoveAccessorDeclaration:
							if (IsAssignmentMeaningful(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.SubtractAssignmentExpression))
							{
								return true;
							}
							break;

						default:
							return true;
					}
				}

				return false;

				bool IsAssignmentMeaningful(ExpressionSyntax expression, SyntaxKind kind)
				{
					if (!expression.IsKind(kind))
					{
						return true;
					}

					var assignment = (AssignmentExpressionSyntax)expression;

					if (!IsValue(assignment.Right))
					{
						return true;
					}

					var propertyRef = (MemberAccessExpressionSyntax)assignment.Left;

					if (!IsBaseMemberAccess(assignment.Left) || !IsMatch(node, propertyRef))
					{
						return true;
					}

					return false;
				}
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
			{
				if (node.ExpressionBody != null)
				{
					return node.ExpressionBody.Accept(this);
				}
				else if (node.Body != null)
				{
					return node.Body.Accept(this);
				}
				else
				{
					return null;
				}
			}

			public override ExpressionSyntax VisitAccessorDeclaration(AccessorDeclarationSyntax node)
			{
				if (node.ExpressionBody != null)
				{
					return node.ExpressionBody.Accept(this);
				}
				else if (node.Body != null)
				{
					return node.Body.Accept(this);
				}
				else
				{
					return null;
				}
			}

			public override ExpressionSyntax VisitBlock(BlockSyntax node)
			{
				if (node.Statements.Count == 1)
				{
					return node.Statements[0].Accept(this);
				}

				return null;
			}

			public override ExpressionSyntax VisitReturnStatement(ReturnStatementSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitExpressionStatement(ExpressionStatementSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitArrowExpressionClause(ArrowExpressionClauseSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression?.Accept(this);

			public override ExpressionSyntax VisitAssignmentExpression(AssignmentExpressionSyntax node) => node;
			public override ExpressionSyntax VisitElementAccessExpression(ElementAccessExpressionSyntax node) => node;
			public override ExpressionSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => node;

			public override ExpressionSyntax VisitBinaryExpression(BinaryExpressionSyntax node) => node;
			public override ExpressionSyntax VisitInvocationExpression(InvocationExpressionSyntax node) => node;
		}
	}
}
