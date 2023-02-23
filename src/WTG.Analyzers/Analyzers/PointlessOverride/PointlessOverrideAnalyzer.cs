using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class PointlessOverrideAnalyzer : DiagnosticAnalyzer
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

			if (IsUnsealedOverride(node) && !node.Accept(IsMeaningfulVisitor.Instance))
			{
				context.ReportDiagnostic(CreateDiagnostic(node));
			}
		}

		static Diagnostic CreateDiagnostic(CSharpSyntaxNode node)
		{
			return node.Kind() switch
			{
				SyntaxKind.PropertyDeclaration => Rules.CreateRemovePointlessOverrides_PropertyDiagnostic(node.GetLocation()),
				SyntaxKind.MethodDeclaration => Rules.CreateRemovePointlessOverrides_MethodDiagnostic(node.GetLocation()),
				SyntaxKind.IndexerDeclaration => Rules.CreateRemovePointlessOverrides_IndexerDiagnostic(node.GetLocation()),
				SyntaxKind.EventDeclaration => Rules.CreateRemovePointlessOverrides_EventDiagnostic(node.GetLocation()),

				// shouldn't happen, but just in case.
				_ => Rules.CreateRemovePointlessOverridesDiagnostic(node.GetLocation()),
			};
		}

		static bool IsUnsealedOverride(CSharpSyntaxNode node)
		{
			var modifiers = node.Accept(ModifierExtractionVisitor.Instance);

			var hasOverrideModifier = false;

			foreach (var modifier in modifiers)
			{
				switch (modifier.Kind())
				{
					case SyntaxKind.OverrideKeyword:
						hasOverrideModifier = true;
						break;

					case SyntaxKind.SealedKeyword:
						return false;
				}
			}

			return hasOverrideModifier;
		}

		static bool IsBaseMemberAccess(MemberAccessExpressionSyntax node)
			=> node.Expression.IsKind(SyntaxKind.BaseExpression);

		static bool IsKind([NotNullWhen(true)] SyntaxNode? node, SyntaxKind kind) => node.IsKind(kind);

		sealed class IsMeaningfulVisitor : CSharpSyntaxVisitor<bool>
		{
			public static IsMeaningfulVisitor Instance { get; } = new IsMeaningfulVisitor();

			IsMeaningfulVisitor()
			{
			}

			public override bool DefaultVisit(SyntaxNode node) => true;

			public override bool VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				if (node.AttributeLists.Any())
				{
					return true;
				}

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
				if (node.AttributeLists.Any())
				{
					return true;
				}

				if (node.ExpressionBody != null)
				{
					return !IsMatchingSelf(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				var accessorList = node.AccessorList;

				if (accessorList == null)
				{
					return true;
				}

				foreach (var accessor in accessorList.Accessors)
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

				bool IsMatchingSelf(ExpressionSyntax? expression)
				{
					if (!IsKind(expression, SyntaxKind.SimpleMemberAccessExpression))
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
				if (node.AttributeLists.Any())
				{
					return true;
				}

				if (node.ExpressionBody != null)
				{
					return !IsMatchingSelf(node.ExpressionBody.Accept(SolitaryExpressionLocator.Instance));
				}

				var accessorList = node.AccessorList;

				if (accessorList == null)
				{
					return true;
				}

				foreach (var accessor in accessorList.Accessors)
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

				bool IsMatchingSelf(ExpressionSyntax? expression)
				{
					if (!IsKind(expression, SyntaxKind.ElementAccessExpression))
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
				if (node.AttributeLists.Any())
				{
					return true;
				}

				var accessorList = node.AccessorList;

				if (accessorList == null)
				{
					return true;
				}

				foreach (var accessor in accessorList.Accessors)
				{
					switch (accessor.Kind())
					{
						case SyntaxKind.AddAccessorDeclaration:
							{
								var expression = GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.AddAssignmentExpression);

								if (!IsMatchingSelf(expression))
								{
									return true;
								}
							}
							break;

						case SyntaxKind.RemoveAccessorDeclaration:
							{
								var expression = GetAssignmentTarget(accessor.Accept(SolitaryExpressionLocator.Instance), SyntaxKind.SubtractAssignmentExpression);

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

				bool IsMatchingSelf(ExpressionSyntax? expression)
				{
					if (!IsKind(expression, SyntaxKind.SimpleMemberAccessExpression))
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

			static ExpressionSyntax? GetAssignmentTarget(ExpressionSyntax? expression, SyntaxKind kind = SyntaxKind.SimpleAssignmentExpression)
			{
				if (IsKind(expression, kind))
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

		sealed class SolitaryExpressionLocator : CSharpSyntaxVisitor<ExpressionSyntax?>
		{
			public static SolitaryExpressionLocator Instance { get; } = new SolitaryExpressionLocator();

			SolitaryExpressionLocator()
			{
			}

			public override ExpressionSyntax? DefaultVisit(SyntaxNode node) => null;

			public override ExpressionSyntax? VisitMethodDeclaration(MethodDeclarationSyntax node)
				=> node.ExpressionBody?.Accept(this) ?? node.Body?.Accept(this);

			public override ExpressionSyntax? VisitAccessorDeclaration(AccessorDeclarationSyntax node)
				=> node.ExpressionBody?.Accept(this) ?? node.Body?.Accept(this);

			public override ExpressionSyntax? VisitBlock(BlockSyntax node)
			{
				if (node.Statements.Count == 1)
				{
					return node.Statements[0].Accept(this);
				}

				return null;
			}

			public override ExpressionSyntax? VisitAwaitExpression(AwaitExpressionSyntax node)
			{
				if (node.Expression.IsKind(SyntaxKind.InvocationExpression))
				{
					var invoke = (InvocationExpressionSyntax)node.Expression;

					if (invoke.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						var member = (MemberAccessExpressionSyntax)invoke.Expression;

						if (member.Name.Identifier.Text == nameof(Task.ConfigureAwait))
						{
							return member.Expression.Accept(this);
						}
					}
				}

				return node.Expression.Accept(this);
			}

			public override ExpressionSyntax? VisitArrowExpressionClause(ArrowExpressionClauseSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax? VisitExpressionStatement(ExpressionStatementSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax? VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression?.Accept(this);
			public override ExpressionSyntax? VisitReturnStatement(ReturnStatementSyntax node) => node.Expression?.Accept(this);

			public override ExpressionSyntax VisitAssignmentExpression(AssignmentExpressionSyntax node) => node;
			public override ExpressionSyntax VisitElementAccessExpression(ElementAccessExpressionSyntax node) => node;
			public override ExpressionSyntax VisitInvocationExpression(InvocationExpressionSyntax node) => node;
			public override ExpressionSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => node;
		}
	}
}
