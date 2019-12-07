using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class SuppressionAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.RemovedOrphanedSuppressionsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
		}

		static void Analyze(SyntaxNodeAnalysisContext context)
		{
			SuppressionTargetLookup lookup = null;

			foreach (var attribute in FindGlobalSuppressionAttributes((CompilationUnitSyntax)context.Node, context.SemanticModel))
			{
				if (TryDecodeAttribute(attribute, out var scope, out var target))
				{
					if (lookup == null)
					{
						lookup = new SuppressionTargetLookup(context.SemanticModel);
					}

					switch (scope)
					{
						case SuppressionScope.Namespace:
							if (!lookup.NamespaceExists(target))
							{
								context.ReportDiagnostic(
									Rules.CreateRemovedOrphanedSuppressionsDiagnostic(
										AttributeUtils.GetLocation(attribute),
										"namespace",
										target));
							}
							break;

						case SuppressionScope.Type:
							if (!lookup.TypeExists(target))
							{
								context.ReportDiagnostic(
									Rules.CreateRemovedOrphanedSuppressionsDiagnostic(
										AttributeUtils.GetLocation(attribute),
										"type",
										target));
							}
							break;

						case SuppressionScope.Member:
							if (!lookup.MemberExists(target))
							{
								context.ReportDiagnostic(
									Rules.CreateRemovedOrphanedSuppressionsDiagnostic(
										AttributeUtils.GetLocation(attribute),
										"member",
										target));
							}
							break;
					}
				}
			}
		}

		static IEnumerable<AttributeSyntax> FindGlobalSuppressionAttributes(CompilationUnitSyntax unit, SemanticModel model)
		{
			foreach (var attributeList in unit.AttributeLists)
			{
				foreach (var attribute in attributeList.Attributes)
				{
					var type = model.GetTypeInfo(attribute).Type;

					if (type != null && type.IsMatch("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute"))
					{
						yield return attribute;
					}
				}
			}
		}

		static bool TryDecodeAttribute(AttributeSyntax att, out SuppressionScope scope, out string target)
		{
			if (TryGetStringValue(att.GetPropertyValue(nameof(SuppressMessageAttribute.Scope)), out var scopeStr) &&
				TryGetStringValue(att.GetPropertyValue(nameof(SuppressMessageAttribute.Target)), out var targetStr))
			{
				scope = TranslateScope(scopeStr);

				if (scope != SuppressionScope.Unknown)
				{
					target = targetStr;
					return true;
				}
			}

			scope = SuppressionScope.Unknown;
			target = null;
			return false;
		}

		static bool TryGetStringValue(ExpressionSyntax expression, out string value)
		{
			if (expression is LiteralExpressionSyntax literal)
			{
				value = (string)literal.Token.Value;
				return true;
			}

			value = null;
			return false;
		}

		static SuppressionScope TranslateScope(string scope)
		{
			return scope switch
			{
				"namespace" => SuppressionScope.Namespace,
				"type" => SuppressionScope.Type,
				"member" => SuppressionScope.Member,
				_ => SuppressionScope.Unknown,
			};
		}

		internal enum SuppressionScope
		{
			Unknown,
			Namespace,
			Type,
			Member,
		}
	}
}
