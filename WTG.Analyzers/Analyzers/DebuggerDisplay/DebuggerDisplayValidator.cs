using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WTG.Analyzers
{
	sealed class DebuggerDisplayValidator : CSharpSyntaxVisitor<bool>
	{
		public DebuggerDisplayValidator(SyntaxNodeAnalysisContext context, DebuggerDisplayContext ddc, ExpressionSyntax textExpression)
		{
			this.context = context;
			this.ddc = ddc;
			this.textExpression = textExpression;
		}

		public override bool VisitIdentifierName(IdentifierNameSyntax node)
		{
			var symbol = ddc.GetExpressionSymbolInfo(node);

			if (symbol.Symbol == null)
			{
				ReportMissingMember(node.Identifier, "this object");
				return false;
			}

			return true;
		}

		public override bool VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			if (Visit(node.Expression))
			{
				var symbolInfo = ddc.GetExpressionSymbolInfo(node);

				if (symbolInfo.Symbol == null)
				{
					var type = ddc.GetExpressionTypeInfo(node.Expression);
					ReportMissingMember(node.Name.Identifier, type.Type.MetadataName);
					return false;
				}
			}

			return true;
		}

		public override bool VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			var result = VisitMethodTarget(node.Expression);

			if (!VisitSeparatedArgumentList(node.ArgumentList.Arguments))
			{
				result = false;
			}

			if (result)
			{
				var symbol = ddc.GetExpressionSymbolInfo(node).Symbol;

				if (symbol == null)
				{
					result = false;
				}
			}

			return result;
		}

		public override bool VisitElementAccessExpression(ElementAccessExpressionSyntax node)
		{
			var result = true;

			if (!Visit(node.Expression))
			{
				result = false;
			}

			if (!VisitSeparatedArgumentList(node.ArgumentList.Arguments))
			{
				result = false;
			}

			if (!result)
			{
				var symbolInfo = ddc.GetExpressionSymbolInfo(node);

				if (symbolInfo.Symbol != null)
				{
					var type = ddc.GetExpressionTypeInfo(node.Expression).Type;
					ReportMissingIndexer(type.Name);
				}
			}

			return result;
		}

		bool VisitMethodTarget(ExpressionSyntax node)
		{
			bool result;
			SimpleNameSyntax name;
			ExpressionSyntax? innerExpression;

			switch (node.Kind())
			{
				case SyntaxKind.IdentifierName:
					result = true;
					name = (IdentifierNameSyntax)node;
					innerExpression = null;
					break;

				case SyntaxKind.SimpleMemberAccessExpression:
					var access = (MemberAccessExpressionSyntax)node;
					name = access.Name;
					innerExpression = access.Expression;
					result = Visit(innerExpression);
					break;

				default: return Visit(node);
			}

			if (result && ddc.GetExpressionSymbolInfo(node).CandidateReason != CandidateReason.OverloadResolutionFailure)
			{
				var typeName = innerExpression == null ? "this object" : ddc.GetExpressionTypeInfo(innerExpression).Type.Name;
				ReportMissingMember(name.Identifier, typeName);
				result = false;
			}

			return result;
		}

		bool VisitSeparatedArgumentList(SeparatedSyntaxList<ArgumentSyntax> arguments)
		{
			var result = true;

			for (var i = 0; i < arguments.Count; i++)
			{
				var argument = arguments[i];

				if (!Visit(argument.Expression))
				{
					result = false;
				}
			}

			return result;
		}

		void ReportMissingMember(SyntaxToken identifierName, string typeName)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.DebuggerDisplayCouldNotResolveReference_MemberRule,
					textExpression.GetLocation(),
					identifierName.Text,
					typeName));
		}

		void ReportMissingIndexer(string typeName)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.DebuggerDisplayCouldNotResolveReference_IndexerRule,
					textExpression.GetLocation(),
					typeName));
		}

		readonly SyntaxNodeAnalysisContext context;
		readonly ExpressionSyntax textExpression;
		readonly DebuggerDisplayContext ddc;
	}
}
