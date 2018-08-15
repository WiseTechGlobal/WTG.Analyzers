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

			if (!IsRuleApplicable(context.SemanticModel, decl, context.CancellationToken))
			{
				return;
			}

			var processor = new Processor(context.SemanticModel, context.ReportDiagnostic, context.CancellationToken);
			decl.Accept(processor);
		}

		static bool IsOverride(MemberDeclarationSyntax decl)
		{
			foreach (var modifier in decl.Accept(ModifierExtractionVisitor.Instance))
			{
				if (modifier.IsKind(SyntaxKind.OverrideKeyword))
				{
					return true;
				}
			}

			return false;
		}

		static bool IsRuleApplicable(SemanticModel semanticModel, MemberDeclarationSyntax memberSyntax, CancellationToken cancellationToken)
		{
			if (IsOverride(memberSyntax))
			{
				return false;
			}

			switch (memberSyntax.Kind())
			{
				case SyntaxKind.FieldDeclaration:
					{
						// If any field is externally visible, then they all are.
						// Fields never implement an interface.
						var variables = ((FieldDeclarationSyntax)memberSyntax).Declaration.Variables;
						var symbol = semanticModel.GetDeclaredSymbol(variables[0], cancellationToken);
						return symbol != null && symbol.IsExternallyVisible();
					}

				case SyntaxKind.EventFieldDeclaration:
					{
						// If any event is externally visible, then they all are.
						// Only consider this as implementing an interface if all the events implement an interface.
						var variables = ((EventFieldDeclarationSyntax)memberSyntax).Declaration.Variables;
						var symbol = semanticModel.GetDeclaredSymbol(variables[0], cancellationToken);

						if (symbol == null || !symbol.IsExternallyVisible())
						{
							return false;
						}
						else if (!symbol.ImplementsAnInterface())
						{
							return true;
						}

						for (var i = 1; i < variables.Count; i++)
						{
							symbol = semanticModel.GetDeclaredSymbol(variables[i], cancellationToken);

							if (!symbol.ImplementsAnInterface())
							{
								return true;
							}
						}

						return false;
					}

				default:
					{
						var symbol = semanticModel.GetDeclaredSymbol(memberSyntax, cancellationToken);
						return symbol != null && symbol.IsExternallyVisible() && !symbol.ImplementsAnInterface();
					}
			}
		}

		sealed class Processor : CSharpSyntaxVisitor
		{
			public Processor(SemanticModel model, Action<Diagnostic> report, CancellationToken cancellationToken)
			{
				this.model = model;
				this.report = report;
				this.cancellationToken = cancellationToken;
			}

			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => VisitParameters(node.ParameterList.Parameters);
			public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) => Visit(node.Type);
			public override void VisitFieldDeclaration(FieldDeclarationSyntax node) => Visit(node.Declaration.Type);
			public override void VisitEventDeclaration(EventDeclarationSyntax node) => Visit(node.Type);
			public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => Visit(node.Declaration.Type);

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				VisitParameters(node.ParameterList.Parameters);
				Visit(node.ReturnType);
			}

			public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
				VisitParameters(node.ParameterList.Parameters);
				Visit(node.Type);
			}

			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
			{
				VisitParameters(node.ParameterList.Parameters);
				Visit(node.ReturnType);
			}

			public override void VisitArrayType(ArrayTypeSyntax node) => Visit(node.ElementType);
			public override void VisitPointerType(PointerTypeSyntax node) => Visit(node.ElementType);
			public override void VisitIdentifierName(IdentifierNameSyntax node) => Validate(node);
			public override void VisitQualifiedName(QualifiedNameSyntax node) => Validate(node);

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

			void VisitParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
			{
				foreach (var parameter in parameters)
				{
					Visit(parameter.Type);
				}
			}

			void Validate(TypeSyntax node)
			{
				var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;

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
