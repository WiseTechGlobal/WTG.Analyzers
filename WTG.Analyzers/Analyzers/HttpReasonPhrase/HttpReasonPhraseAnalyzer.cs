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
				SyntaxKind.SimpleAssignmentExpression,
				SyntaxKind.AddAssignmentExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var assignment = (AssignmentExpressionSyntax)context.Node;

			if (assignment.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				HandleSimpleMemberAccessExpression((MemberAccessExpressionSyntax)assignment.Left, context);
			}
			else if (assignment.Left.IsKind(SyntaxKind.IdentifierName) && assignment.Parent.IsKind(SyntaxKind.ObjectInitializerExpression))
			{
				HandleObjectInitializerExpression((InitializerExpressionSyntax)assignment.Parent, (IdentifierNameSyntax)assignment.Left, context);
			}
		}

		static void HandleSimpleMemberAccessExpression(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context)
		{
			if (!IsReasonPhraseIdentifier(memberAccess.Name))
			{
				return;
			}

			var expression = memberAccess.Expression;
			var typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);

			if (!ShouldTriggerDiagnosticForContainingType(typeInfo.Type))
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.ForbidCustomHttpReasonPhraseValuesRule,
					context.Node.GetLocation()));
		}

		static void HandleObjectInitializerExpression(InitializerExpressionSyntax initializer, IdentifierNameSyntax identifier, SyntaxNodeAnalysisContext context)
		{
			if (!IsReasonPhraseIdentifier(identifier))
			{
				return;
			}

			if (!initializer.Parent.IsKind(SyntaxKind.ObjectCreationExpression))
			{
				// We can worry about C# 9 with-ers another time.
				return;
			}

			var creation = (ObjectCreationExpressionSyntax)initializer.Parent;

			var typeInfo = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken);

			if (!ShouldTriggerDiagnosticForContainingType(typeInfo.Type))
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.ForbidCustomHttpReasonPhraseValuesRule,
					context.Node.GetLocation()));
		}

		static bool IsReasonPhraseIdentifier(SimpleNameSyntax identifier) => string.Equals(identifier.Identifier.ValueText, ReasonPhrasePropertyName, StringComparison.Ordinal);

		static bool ShouldTriggerDiagnosticForContainingType(ITypeSymbol typeSymbol) => typeSymbol is { } && (typeSymbol.IsMatch(HttpResponseMessageFullTypeName) || typeSymbol.IsMatch(IHttpResponseFeatureFullTypeName));
	}
}
