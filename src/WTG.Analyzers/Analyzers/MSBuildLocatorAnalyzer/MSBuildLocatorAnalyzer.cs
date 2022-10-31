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
	public sealed partial class MSBuildLocatorAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.ForbidMSBuildLocatorRegisterDefaultsRule);

		const string RegisterDefaultsMethodName = "RegisterDefaults";

		const string MSBuildLocatorFullTypeName = "Microsoft.Build.Locator.MSBuildLocator";
		const string VisualStudioInstanceFullTypeName = "Microsoft.Build.Locator.VisualStudioInstance";

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
				SyntaxKind.SimpleMemberAccessExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var memberAccess = (MemberAccessExpressionSyntax)context.Node;

			if (!string.Equals(memberAccess.Name.Identifier.ValueText, RegisterDefaultsMethodName, StringComparison.Ordinal))
			{
				return;
			}

			// If we're invoking the method, then the Diagnostic should be on the entire invocation (including the parameter list).
			// If we're not invoking the method, then the Diagnostic should only cover the member access itself.
			var syntax = memberAccess.Parent.IsKind(SyntaxKind.InvocationExpression) ? memberAccess.Parent : memberAccess;

			var symbol = context.SemanticModel.GetSymbolInfo(syntax, context.CancellationToken);
			if (symbol.Symbol is null || symbol.Symbol.Kind != SymbolKind.Method)
			{
				return;
			}

			var method = (IMethodSymbol)symbol.Symbol;
			if (method.ReceiverType is null ||
				!method.ReceiverType.IsMatch(MSBuildLocatorFullTypeName) ||
				!method.ReturnType.IsMatch(VisualStudioInstanceFullTypeName) ||
				method.Parameters.Length > 0)
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.ForbidMSBuildLocatorRegisterDefaultsRule,
					syntax.GetLocation()));
		}
	}
}
