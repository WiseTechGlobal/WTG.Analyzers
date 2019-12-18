using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DebuggerDisplayAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.InvalidDebuggerDisplayFormatRule,
			Rules.DebuggerDisplayCouldNotResolveReference_IndexerRule,
			Rules.DebuggerDisplayCouldNotResolveReference_MemberRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.Attribute);
		}

		static void Analyze(SyntaxNodeAnalysisContext context)
		{
			var attribute = (AttributeSyntax)context.Node;
			var type = context.SemanticModel.GetTypeInfo(attribute, context.CancellationToken).Type;

			if (type != null && type.IsMatch("System.Diagnostics.DebuggerDisplayAttribute"))
			{
				var ddc = DebuggerDisplayContext.Create(context.SemanticModel, attribute);

				foreach (var valueExpression in FormatStrings(attribute))
				{
					var valueOpt = context.SemanticModel.GetConstantValue(valueExpression, context.CancellationToken);

					if (valueOpt.HasValue)
					{
						var value = (string)valueOpt.Value;
						var expressions = DebuggerDisplayDetail.Parse(value);

						if (expressions.HasError)
						{
							context.ReportDiagnostic(
								Diagnostic.Create(
									Rules.InvalidDebuggerDisplayFormatRule,
									valueExpression.GetLocation()));
						}

						var validator = new DebuggerDisplayValidator(context, ddc, valueExpression);

						foreach (var expression in expressions.Values)
						{
							validator.Visit(expression);
						}
					}
				}
			}
		}

		static IEnumerable<ExpressionSyntax> FormatStrings(AttributeSyntax attribute)
		{
			var expression = attribute.GetArgumentValue(0);

			if (expression != null)
			{
				yield return expression;
			}

			foreach (var arg in attribute.ArgumentList.Arguments)
			{
				var name = arg.NameEquals?.Name?.Identifier.Text;

				if (name != null)
				{
					switch (name)
					{
						case nameof(DebuggerDisplayAttribute.Name):
						case nameof(DebuggerDisplayAttribute.Type):
							yield return arg.Expression;
							break;
					}
				}
			}
		}
	}
}
