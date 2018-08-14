using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class PublicTupleTypeAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.AvoidTupleTypesInPublicInterfacesRule);

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
				SyntaxKind.ConstructorDeclaration,
				SyntaxKind.MethodDeclaration,
				SyntaxKind.PropertyDeclaration,
				SyntaxKind.IndexerDeclaration,
				SyntaxKind.DelegateDeclaration,
				SyntaxKind.EventDeclaration,
				SyntaxKind.EventFieldDeclaration,
				SyntaxKind.FieldDeclaration);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var decl = (MemberDeclarationSyntax)context.Node;
			var processor = new Processor(context.SemanticModel, context.ReportDiagnostic, context.CancellationToken);
			decl.Accept(processor);
		}

		sealed class Processor : CSharpSyntaxVisitor
		{
			public Processor(SemanticModel model, Action<Diagnostic> report, CancellationToken cancellationToken)
			{
				this.model = model;
				this.report = report;
				this.cancellationToken = cancellationToken;
			}

			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				var syntax = model.GetDeclaredSymbol(node, cancellationToken);

				if (syntax != null && syntax.IsExternallyVisible())
				{
					VisitParameters(node.ParameterList.Parameters);
				}
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				if (!IsOverride(node.Modifiers) &&
					model.GetDeclaredSymbol(node, cancellationToken) is var symbol &&
					symbol != null &&
					symbol.IsExternallyVisible() &&
					!symbol.ImplementsAnInterface())
				{
					VisitParameters(node.ParameterList.Parameters);
					Visit(node.ReturnType);
				}
			}

			public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
			{
				if (!IsOverride(node.Modifiers) &&
					model.GetDeclaredSymbol(node, cancellationToken) is var symbol &&
					symbol != null &&
					symbol.IsExternallyVisible() &&
					!symbol.ImplementsAnInterface())
				{
					Visit(node.Type);
				}
			}

			public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
			{
				var variables = node.Declaration.Variables;

				if (variables.Count > 0 &&
					model.GetDeclaredSymbol(variables[0], cancellationToken) is var symbol &&
					symbol != null &&
					symbol.IsExternallyVisible())
				{
					Visit(node.Declaration.Type);
				}
			}

			public override void VisitEventDeclaration(EventDeclarationSyntax node)
			{
				if (!IsOverride(node.Modifiers) &&
					model.GetDeclaredSymbol(node, cancellationToken) is var symbol &&
					symbol != null &&
					symbol.IsExternallyVisible() &&
					!symbol.ImplementsAnInterface())
				{
					Visit(node.Type);
				}
			}

			public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
			{
				if (!IsOverride(node.Modifiers) && MustComply(node.Declaration))
				{
					Visit(node.Declaration.Type);
				}

				bool MustComply(VariableDeclarationSyntax declaration)
				{
					var variables = node.Declaration.Variables;

					if (variables.Count > 0)
					{
						var syntax = model.GetDeclaredSymbol(variables[0], cancellationToken);

						if (syntax != null && syntax.IsExternallyVisible() && !syntax.ImplementsAnInterface())
						{
							return true;
						}

						for (var i = 1; i < variables.Count; i++)
						{
							syntax = model.GetDeclaredSymbol(variables[0], cancellationToken);

							if (syntax != null && !syntax.ImplementsAnInterface())
							{
								return true;
							}
						}
					}

					return false;
				}
			}

			public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
				if (!IsOverride(node.Modifiers) &&
					model.GetDeclaredSymbol(node, cancellationToken) is var symbol &&
					symbol != null &&
					symbol.IsExternallyVisible() &&
					!symbol.ImplementsAnInterface())
				{
					VisitParameters(node.ParameterList.Parameters);
					Visit(node.Type);
				}
			}

			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
			{
				var syntax = model.GetDeclaredSymbol(node, cancellationToken);

				if (syntax != null && syntax.IsExternallyVisible())
				{
					VisitParameters(node.ParameterList.Parameters);
					Visit(node.ReturnType);
				}
			}

			public override void VisitArrayType(ArrayTypeSyntax node)
			{
				Visit(node.ElementType);
			}

			public override void VisitPointerType(PointerTypeSyntax node)
			{
				Visit(node.ElementType);
			}

			public override void VisitGenericName(GenericNameSyntax node)
			{
				foreach (var arg in node.TypeArgumentList.Arguments)
				{
					Visit(arg);
				}

				Validate(node);
			}

			public override void VisitTupleType(TupleTypeSyntax node)
			{
				foreach (var element in node.Elements)
				{
					Visit(element.Type);
				}

				Validate(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				Validate(node);
			}

			public override void VisitQualifiedName(QualifiedNameSyntax node)
			{
				Validate(node);
			}

			static bool IsOverride(SyntaxTokenList modifiers)
			{
				foreach (var modifier in modifiers)
				{
					if (modifier.IsKind(SyntaxKind.OverrideKeyword))
					{
						return true;
					}
				}

				return false;
			}

			void VisitParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
			{
				foreach (var parameter in parameters)
				{
					Visit(parameter.Type);
				}
			}

			void Validate(TypeSyntax node)
			{
				var symbol = model.GetSymbolInfo(node).Symbol;

				if (symbol != null &&
					symbol.Kind == SymbolKind.NamedType &&
					IsTupleLike((INamedTypeSymbol)symbol))
				{
					report(Diagnostic.Create(
						Rules.AvoidTupleTypesInPublicInterfacesRule,
						node.GetLocation()));
				}
			}

			static bool IsTupleLike(INamedTypeSymbol symbol)
			{
				if (symbol.IsTupleType)
				{
					return true;
				}

				switch (symbol.TypeKind)
				{
					case TypeKind.Class:
						return symbol.IsMatchAnyArity("System.Tuple");

					case TypeKind.Struct:
						return symbol.IsMatchAnyArity("System.ValueTuple");
				}

				return false;
			}

			readonly SemanticModel model;
			readonly Action<Diagnostic> report;
			readonly CancellationToken cancellationToken;
		}
	}
}
