using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class FileSystemPathsAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotUsePathSeparatorsInPathLiteralsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			if (!context.Compilation.IsCSharpVersionOrGreater(LanguageVersion.CSharp4))
			{
				return;
			}

			var hasPathCombineMulti = HasPathCombineMulti(context.Compilation);
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache, hasPathCombineMulti),
				SyntaxKind.InvocationExpression);
		}

		static bool HasPathCombineMulti(Compilation compilation)
		{
			var ioPathSymbol = compilation.GetTypeByMetadataName("System.IO.Path");

			if (ioPathSymbol != null)
			{
				foreach (var symbol in ioPathSymbol.GetMembers(nameof(Path.Combine)))
				{
					if (symbol.Kind != SymbolKind.Method)
					{
						continue;
					}

					var method = (IMethodSymbol)symbol;
					if (method.Parameters.Length != 1)
					{
						continue;
					}

					// We need the "params string[]" overload to be available
					var parameter = method.Parameters[0];
					if (!parameter.IsParams || parameter.Type.TypeKind != TypeKind.Array || ((IArrayTypeSymbol)parameter.Type).ElementType.SpecialType != SpecialType.System_String)
					{
						continue;
					}

					return true;
				}
			}

			return false;
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache, bool hasPathCombineMulti)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var invocation = (InvocationExpressionSyntax)context.Node;

			if (LooksLikePathCombine(invocation))
			{
				if (invocation.ArgumentList.Arguments.Count >= 2 && hasPathCombineMulti)
				{
					var arguments = invocation.ArgumentList.Arguments;
					for (var i = 0; i < arguments.Count; i++)
					{
						if (!ExpressionLooksSuspicious(arguments[i].Expression, isFirst: i == 0))
						{
							continue;
						}

						if (!IsPathCombine(context.SemanticModel, invocation, context.CancellationToken))
						{
							// This isn't actually Path.Combine so finish up the loop.
							break;
						}

						for (var j = 0; j < arguments.Count; j++)
						{
							var argument = arguments[j];
							var result = ExpressionIsSuspicious(context.SemanticModel, argument.Expression, isFirst: j == 0, context.CancellationToken);
							if (result == SuspicionResult.LooksOk)
							{
								continue;
							}

							context.ReportDiagnostic(CreateDiagnostic(argument.Expression, result == SuspicionResult.CanAutoFix));
						}

						// We've examined all arguments against the semantic model now, so finish up the loop.
						break;
					}
				}
				else if (invocation.ArgumentList.Arguments.Count == 1 && invocation.ArgumentList.Arguments[0] is var argument)
				{
					InitializerExpressionSyntax? initializer;

					if (argument.Expression.IsKind(SyntaxKind.ArrayCreationExpression))
					{
						var arrayCreation = (ArrayCreationExpressionSyntax)argument.Expression;
						initializer = arrayCreation.Initializer;
					}
					else if (argument.Expression.IsKind(SyntaxKind.ImplicitArrayCreationExpression))
					{
						var arrayCreation = (ImplicitArrayCreationExpressionSyntax)argument.Expression;
						initializer = arrayCreation.Initializer;
					}
					else
					{
						initializer = null;
					}

					if (initializer != null)
					{
						var expressions = initializer.Expressions;

						for (var i = 0; i < expressions.Count; i++)
						{
							var expression = expressions[i];
							if (!ExpressionLooksSuspicious(expression, isFirst: i == 0))
							{
								continue;
							}

							if (!IsPathCombine(context.SemanticModel, invocation, context.CancellationToken))
							{
								// This isn't actually Path.Combine so finish up the loop.
								break;
							}

							for (var j = 0; j < expressions.Count; j++)
							{
								expression = expressions[j];
								var result = ExpressionIsSuspicious(context.SemanticModel, expression, isFirst: j == 0, context.CancellationToken);
								if (result == SuspicionResult.LooksOk)
								{
									continue;
								}

								context.ReportDiagnostic(CreateDiagnostic(expression, result == SuspicionResult.CanAutoFix));
							}

							// We've examined all expressions against the semantic model now, so finish up the loop.
							break;
						}
					}
				}
			}
		}

		static Diagnostic CreateDiagnostic(SyntaxNode syntaxNode, bool canAutoFix)
		{
			return Diagnostic.Create(
				Rules.DoNotUsePathSeparatorsInPathLiteralsRule,
				syntaxNode.GetLocation(),
				canAutoFix
					? ImmutableDictionary<string, string?>.Empty
					: CommonDiagnosticProperties.NoAutoFixProperties);
		}

		static bool LooksLikePathCombine(InvocationExpressionSyntax syntax)
		{
			if (syntax.Expression is null || !syntax.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				return false;
			}

			var smae = (MemberAccessExpressionSyntax)syntax.Expression;
			if (smae.Name.Identifier.Text != nameof(Path.Combine))
			{
				return false;
			}

			return true;
		}

		static bool ExpressionLooksSuspicious(ExpressionSyntax expression, bool isFirst)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)expression;
					return StringLiteralExpressionIsSuspicious(literal, isFirst);

				case SyntaxKind.AddExpression:
					var add = (BinaryExpressionSyntax)expression;
					return ExpressionLooksSuspicious(add.Left, isFirst) || ExpressionLooksSuspicious(add.Right, isFirst: false);

				case SyntaxKind.InterpolatedStringExpression:
					var interpolatedString = (InterpolatedStringExpressionSyntax)expression;
					return InterpolatedStringIsSuspicious(interpolatedString, isFirst);

				case SyntaxKind.IdentifierName:
					return true;

				default:
					return false;
			}
		}

		static SuspicionResult ExpressionIsSuspicious(SemanticModel semanticModel, ExpressionSyntax expression, bool isFirst, CancellationToken cancellationToken)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.StringLiteralExpression:
					var literal = (LiteralExpressionSyntax)expression;
					return StringLiteralExpressionIsSuspicious(literal, isFirst) ? SuspicionResult.CanAutoFix : SuspicionResult.LooksOk;

				case SyntaxKind.AddExpression:
					var add = (BinaryExpressionSyntax)expression;
					var result1 = ExpressionIsSuspicious(semanticModel, add.Left, isFirst, cancellationToken);

					if (result1 == SuspicionResult.CanAutoFix)
					{
						return result1;
					}

					var result2 = ExpressionIsSuspicious(semanticModel, add.Right, isFirst: false, cancellationToken);

					return result1 > result2 ? result1 : result2;

				case SyntaxKind.InterpolatedStringExpression:
					var interpolatedString = (InterpolatedStringExpressionSyntax)expression;
					return InterpolatedStringIsSuspicious(interpolatedString, isFirst) ? SuspicionResult.CanAutoFix : SuspicionResult.LooksOk;

				case SyntaxKind.IdentifierName:
					return IdentifierNameIsSuspicious(semanticModel, expression, isFirst, cancellationToken) ? SuspicionResult.CannotAutoFix : SuspicionResult.LooksOk;

				default:
					return SuspicionResult.LooksOk;
			}
		}

		static bool StringLiteralExpressionIsSuspicious(LiteralExpressionSyntax syntax, bool isFirst)
		{
			var text = syntax.Token.ValueText;
			return TextContainsPathSeparator(text, isFirst);
		}

		static bool InterpolatedStringIsSuspicious(InterpolatedStringExpressionSyntax syntax, bool isFirst)
		{
			var contents = syntax.Contents;
			for (var i = 0; i < contents.Count; i++)
			{
				var section = contents[i];
				if (!section.IsKind(SyntaxKind.InterpolatedStringText))
				{
					continue;
				}

				var text = (InterpolatedStringTextSyntax)section;
				if (TextContainsPathSeparator(text.TextToken.ValueText, isFirst && i == 0))
				{
					return true;
				}
			}

			return false;
		}

		static bool IdentifierNameIsSuspicious(SemanticModel semanticModel, ExpressionSyntax expression, bool isFirst, CancellationToken cancellationToken)
		{
			var constant = semanticModel.GetConstantValue(expression, cancellationToken);
			if (!constant.HasValue || !(constant.Value is string value))
			{
				return false;
			}

			return TextContainsPathSeparator(value, isFirst);
		}

		static bool TextContainsPathSeparator(string text, bool isFirst)
		{
			if (isFirst && IsLikelyWellKnownPlatformSpecificPath(text))
			{
				return false;
			}

			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '/' || text[i] == '\\')
				{
					return true;
				}
			}

			return false;
		}

		static bool IsLikelyWellKnownPlatformSpecificPath(string text)
		{
			if (text.Length == 0)
			{
				// No text to examine
				return false;
			}

			if (text[0] == '/')
			{
				// likely UNIX-style path (/blah/foo)
				return true;
			}

			if (text.Length > 1)
			{
				if (text[0] == '\\' && text[1] == '\\')
				{
					// likely Windows UNC path or extended path (\\server\share or \\?\ prefix)
					return true;
				}

				if (char.IsLetter(text[0]) && text[1] == ':')
				{
					// likely Windows local path with drive letter (X:\path)
					return true;
				}
			}

			return false;
		}

		static bool IsPathCombine(SemanticModel semanticModel, InvocationExpressionSyntax syntax, CancellationToken cancellationToken)
		{
			var symbol = (IMethodSymbol?)semanticModel.GetSymbolInfo(syntax, cancellationToken).Symbol;
			if (symbol is null || !symbol.ContainingType.IsMatch("System.IO.Path") || symbol.Parameters.Length == 0)
			{
				return false;
			}

			if (symbol.Parameters.Length == 1)
			{
				var parameter = symbol.Parameters[0];
				if (parameter.Type.TypeKind != TypeKind.Array || ((IArrayTypeSymbol)parameter.Type).ElementType.SpecialType != SpecialType.System_String)
				{
					return false;
				}
			}
			else
			{
				foreach (var parameter in symbol.Parameters)
				{
					if (parameter.Type.SpecialType != SpecialType.System_String)
					{
						return false;
					}
				}
			}

			return true;
		}

		enum SuspicionResult
		{
			/// <summary>
			/// Did not identify anything wrong with the expression.
			/// </summary>
			LooksOk,

			/// <summary>
			/// Identified one or more issues, but all need to be fixed manually.
			/// </summary>
			CannotAutoFix,

			/// <summary>
			/// Identified one or more issues, at least some can be resolved automatically.
			/// </summary>
			CanAutoFix,
		}
	}
}
