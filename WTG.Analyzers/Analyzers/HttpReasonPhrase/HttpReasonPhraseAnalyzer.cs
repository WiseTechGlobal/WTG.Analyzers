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
	public sealed partial class HttpReasonPhraseAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.ForbidCustomHttpReasonPhraseValuesRule);

		const string ReasonPhrasePropertyName = "ReasonPhrase";

		const string HttpResponseMessageFullTypeName = "System.Net.Http.HttpResponseMessage";
		const string IHttpResponseFeatureFullTypeName = "Microsoft.AspNetCore.Http.Features.IHttpResponseFeature";

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.SimpleAssignmentExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var assignment = (AssignmentExpressionSyntax)context.Node;

			if (!assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
			{
				return;
			}

			if (!assignment.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				return;
			}

			var memberAccess = (MemberAccessExpressionSyntax)assignment.Left;

			if (!memberAccess.OperatorToken.IsKind(SyntaxKind.DotToken))
			{
				return;
			}

			if (!string.Equals(memberAccess.Name.Identifier.ValueText, ReasonPhrasePropertyName, StringComparison.Ordinal))
			{
				return;
			}

			var expression = memberAccess.Expression;
			var typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);

			if (typeInfo.Type is null)
			{
				// Type could not be found.
				return;
			}

			if (!typeInfo.Type.IsMatch(HttpResponseMessageFullTypeName) && !typeInfo.Type.IsMatch(IHttpResponseFeatureFullTypeName))
			{
				// These are not the droids that we are looking for.
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.ForbidCustomHttpReasonPhraseValuesRule,
					assignment.GetLocation()));
		}
	}
}
