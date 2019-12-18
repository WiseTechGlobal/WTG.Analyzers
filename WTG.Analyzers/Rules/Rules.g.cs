﻿// <auto-generated />
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers
{
	public static class Rules
	{
		public const string CodingConventionCategory = "CodingConvention";
		public const string CorrectnessCategory = "Correctness";
		public const string DecruftificationCategory = "Decruftification";
		public const string MaintainabilityCategory = "Maintainability";
		public const string DoNotUseThePrivateKeywordDiagnosticID = "WTG1001";
		public const string UseVarWherePossibleDiagnosticID = "WTG1002";
		public const string DoNotLeaveWhitespaceOnTheEndOfTheLineDiagnosticID = "WTG1003";
		public const string IndentWithTabsRatherThanSpacesDiagnosticID = "WTG1004";
		public const string UseConsistentLineEndingsDiagnosticID = "WTG1005";
		public const string DoNotUseTheInternalKeywordForTopLevelTypesDiagnosticID = "WTG1006";
		public const string DoNotCompareBoolToAConstantValueDiagnosticID = "WTG1007";
		public const string DoNotCompareBoolToAConstantValueInAnExpressionDiagnosticID = "WTG1008";
		public const string UsingDirectivesMustBeOrderedByKindDiagnosticID = "WTG1009";
		public const string UseOutVarWherePossibleDiagnosticID = "WTG1010";
		public const string DeconstructWithSingleVarDiagnosticID = "WTG1011";
		public const string DeconstructWithVarDiagnosticID = "WTG1012";
		public const string AvoidTupleTypesInPublicInterfacesDiagnosticID = "WTG1013";
		public const string DontNestConditionalOperatorsDiagnosticID = "WTG1014";
		public const string ConditionalOperatorsShouldNotHaveMultilineValuesDiagnosticID = "WTG1015";
		public const string AvoidDiscardCoalesceThrowDiagnosticID = "WTG1016";
		public const string DoNotConfigureAwaitFromAsyncVoidDiagnosticID = "WTG2001";
		public const string AvoidConditionalCompilationBasedOnDebugDiagnosticID = "WTG2002";
		public const string FlagEnumsShouldSpecifyExplicitValuesDiagnosticID = "WTG2003";
		public const string DoNotUseCodeContractsDiagnosticID = "WTG2004";
		public const string RemovedOrphanedSuppressionsDiagnosticID = "WTG3001";
		public const string PreferDirectMemberAccessOverLinqDiagnosticID = "WTG3002";
		public const string PreferDirectMemberAccessOverLinqInAnExpressionDiagnosticID = "WTG3003";
		public const string PreferArrayEmptyOverNewArrayConstructionDiagnosticID = "WTG3004";
		public const string DontCallToStringOnAStringDiagnosticID = "WTG3005";
		public const string PreferNameofOverCallingToStringOnAnEnumDiagnosticID = "WTG3006";
		public const string RemovePointlessOverridesDiagnosticID = "WTG3007";
		public const string DontEquateValueTypesWithNullDiagnosticID = "WTG3008";
		public const string PreferCompletedTaskDiagnosticID = "WTG3009";
		public const string DontAwaitTriviallyCompletedTasksDiagnosticID = "WTG3010";
		public const string DoNotNestRegionsDiagnosticID = "WTG3101";
		public const string RegionsShouldNotSplitStructuresDiagnosticID = "WTG3102";
		public const string ConditionalCompilationDirectivesShouldNotSplitStructuresDiagnosticID = "WTG3103";

		public static readonly DiagnosticDescriptor DoNotUseThePrivateKeywordRule = new DiagnosticDescriptor(
			DoNotUseThePrivateKeywordDiagnosticID,
			"Do not use the 'private' keyword.",
			"Our convention is to omit the 'private' modifier where it is already the default.",
			CodingConventionCategory,
			DiagnosticSeverity.Hidden,
			isEnabledByDefault: true,
			description: "Remove the 'private' keyword.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor UseVarWherePossibleRule = new DiagnosticDescriptor(
			UseVarWherePossibleDiagnosticID,
			"Use the 'var' keyword instead of an explicit type where possible.",
			"The compiler is able to correctly identify which type to use here, so replace the explicit type with var.",
			CodingConventionCategory,
			DiagnosticSeverity.Hidden,
			isEnabledByDefault: true,
			description: "Replace with the 'var' keyword.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DoNotLeaveWhitespaceOnTheEndOfTheLineRule = new DiagnosticDescriptor(
			DoNotLeaveWhitespaceOnTheEndOfTheLineDiagnosticID,
			"Do not leave whitespace on the end of the line.",
			"You have meaningless whitespace on the end of the line, remove it.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Remove whitespace from the end of the line.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor IndentWithTabsRatherThanSpacesRule = new DiagnosticDescriptor(
			IndentWithTabsRatherThanSpacesDiagnosticID,
			"Indent with tabs rather than spaces.",
			"Our coding convention is to use tabs, not spaces, you may need to fix your settings.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Replace the leading spaces with tabs.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor UseConsistentLineEndingsRule = new DiagnosticDescriptor(
			UseConsistentLineEndingsDiagnosticID,
			"Use consistent line endings.",
			"All line endings should be using CRLF, this issue usually occures when copying code from another source.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Replace the line ending character sequence with CRLF.");

		public static readonly DiagnosticDescriptor DoNotUseTheInternalKeywordForTopLevelTypesRule = new DiagnosticDescriptor(
			DoNotUseTheInternalKeywordForTopLevelTypesDiagnosticID,
			"Do not use the 'internal' keyword for non-nested type definitions.",
			"Our convention is to omit the 'internal' modifier on types where it is already the default.",
			CodingConventionCategory,
			DiagnosticSeverity.Hidden,
			isEnabledByDefault: true,
			description: "Remove the 'internal' keyword.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DoNotCompareBoolToAConstantValueRule = new DiagnosticDescriptor(
			DoNotCompareBoolToAConstantValueDiagnosticID,
			"Do not compare bool to a constant value.",
			"Do not compare bool to a constant value.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Either use the original value as-is, or use a logical-not operator ('!')",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DoNotCompareBoolToAConstantValueInAnExpressionRule = new DiagnosticDescriptor(
			DoNotCompareBoolToAConstantValueInAnExpressionDiagnosticID,
			"Do not compare bool to a constant value in an expression.",
			"Do not compare bool to a constant value in an expression.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Either use the original value as-is, or use a logical-not operator ('!')",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor UsingDirectivesMustBeOrderedByKindRule = new DiagnosticDescriptor(
			UsingDirectivesMustBeOrderedByKindDiagnosticID,
			"Using directives must be ordered by kind.",
			"'using' directives should be placed before 'using static' directives, which in turn must be placed before 'using' alias directives.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Move all 'using static' statements after 'using' statements, and move 'using X=Y' to the end.");

		public static readonly DiagnosticDescriptor UseOutVarWherePossibleRule = new DiagnosticDescriptor(
			UseOutVarWherePossibleDiagnosticID,
			"Use the 'var' keyword instead of an explicit type where possible.",
			"The compiler is able to correctly identify which type to use here, so replace the explicit type with var.",
			CodingConventionCategory,
			DiagnosticSeverity.Hidden,
			isEnabledByDefault: true,
			description: "Replace with the 'var' keyword.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DeconstructWithSingleVarRule = new DiagnosticDescriptor(
			DeconstructWithSingleVarDiagnosticID,
			"Use the 'var' keyword once when deconstructing an object.",
			"Only declare 'var' once when deconstructing an object, if applicable.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Move 'var' outside of the deconstruction expression.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DeconstructWithVarRule = new DiagnosticDescriptor(
			DeconstructWithVarDiagnosticID,
			"Use the 'var' keyword instead of an explicit type where possible.",
			"The compiler is able to correctly identify which type to use here, so replace the explicit type with var.",
			CodingConventionCategory,
			DiagnosticSeverity.Hidden,
			isEnabledByDefault: true,
			description: "Replace with the 'var' keyword.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor AvoidTupleTypesInPublicInterfacesRule = new DiagnosticDescriptor(
			AvoidTupleTypesInPublicInterfacesDiagnosticID,
			"Don't use tuple types in public interfaces.",
			"Tuple types don't impart useful semantic information, public interfaces should use a dedicated type instead.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Replace with a dedicated type.");

		public static readonly DiagnosticDescriptor DontNestConditionalOperatorsRule = new DiagnosticDescriptor(
			DontNestConditionalOperatorsDiagnosticID,
			"Don't nest conditional operators.",
			"Nesting conditional operators makes the code harder to read, use an 'if' statement instead.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Replace the outer conditional operator with an 'if' statement.");

		public static readonly DiagnosticDescriptor ConditionalOperatorsShouldNotHaveMultilineValues_WhenTrueRule = new DiagnosticDescriptor(
			ConditionalOperatorsShouldNotHaveMultilineValuesDiagnosticID,
			"Conditional operators should not have multiline values.",
			"The true operand of a conditional operator should occupy a single line, the same line as the '?' symbol.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "If you cannot fit the operand on a single line, then an 'if' statement will be a more readable option.");

		public static readonly DiagnosticDescriptor ConditionalOperatorsShouldNotHaveMultilineValues_WhenFalseRule = new DiagnosticDescriptor(
			ConditionalOperatorsShouldNotHaveMultilineValuesDiagnosticID,
			"Conditional operators should not have multiline values.",
			"The false operand of a conditional operator should occupy a single line, the same line as the ':' symbol.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "If you cannot fit the operand on a single line, then an 'if' statement will be a more readable option.");

		public static readonly DiagnosticDescriptor AvoidDiscardCoalesceThrowRule = new DiagnosticDescriptor(
			AvoidDiscardCoalesceThrowDiagnosticID,
			"Avoid the discard-coalesce-throw pattern.",
			"Prefer an if-throw over assigning a coalesce-throw expression to the discard symbol.",
			CodingConventionCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Prefer an if-throw over assigning a coalesce-throw expression to the discard symbol.");

		public static readonly DiagnosticDescriptor DoNotConfigureAwaitFromAsyncVoidRule = new DiagnosticDescriptor(
			DoNotConfigureAwaitFromAsyncVoidDiagnosticID,
			"Do not use ConfigureAwait from an async void method.",
			"ConfigureAwait(false) may result in the async method resuming on a non-deterministic thread, and if an exception is then thrown, it will likely be unhandled and result in process termination.",
			CorrectnessCategory,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Remove the ConfigureAwait call, or return Task.");

		public static readonly DiagnosticDescriptor AvoidConditionalCompilationBasedOnDebugRule = new DiagnosticDescriptor(
			AvoidConditionalCompilationBasedOnDebugDiagnosticID,
			"Avoid conditional compilation based on DEBUG.",
			"Avoid referencing DEBUG in #if or #elif.",
			CorrectnessCategory,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Changing the behaviour in debug vs release means our tests are not testing what the user sees. Consider using debug switches or command line arguments instead.");

		public static readonly DiagnosticDescriptor FlagEnumsShouldSpecifyExplicitValuesRule = new DiagnosticDescriptor(
			FlagEnumsShouldSpecifyExplicitValuesDiagnosticID,
			"Flags enums should specify explicit values.",
			"This member does not specify an explicit value.",
			CorrectnessCategory,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "The auto-generated values for enums don't work well for flag enums, so you should specify the value explicitly.");

		public static readonly DiagnosticDescriptor DoNotUseCodeContractsRule = new DiagnosticDescriptor(
			DoNotUseCodeContractsDiagnosticID,
			"This project does not use Code Contracts.",
			"This project does not use Code Contracts.",
			CorrectnessCategory,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "References to Code Contracs should be replaced with alternate forms of checking or should be deleted.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RemovedOrphanedSuppressionsRule = new DiagnosticDescriptor(
			RemovedOrphanedSuppressionsDiagnosticID,
			"Remove orphaned suppressions.",
			"Encountered a code analysis suppression for the non-existent {0} '{1}'. Remove or update it.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "If you change or remove a type or member that had a code analysis suppression against it, be sure to remove any orphaned suppression attributes. This is usually easier to maintain if the suppression attributes are applied directly to the type/member rather than applied to the assembly in a GlobalSuppressions.cs file.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor PreferDirectMemberAccessOverLinq_UsePropertyRule = new DiagnosticDescriptor(
			PreferDirectMemberAccessOverLinqDiagnosticID,
			"Prefer direct member access over linq.",
			"Don't use the {0} extension method on a source of type '{1}', use the '{2}' property instead.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Don't use linq extension methods when there is a better alternative.");

		public static readonly DiagnosticDescriptor PreferDirectMemberAccessOverLinq_UseIndexerRule = new DiagnosticDescriptor(
			PreferDirectMemberAccessOverLinqDiagnosticID,
			"Prefer direct member access over linq.",
			"Don't use the {0} extension method on a source of type '{1}', use the indexer instead.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Don't use linq extension methods when there is a better alternative.");

		public static readonly DiagnosticDescriptor PreferDirectMemberAccessOverLinqInAnExpression_UsePropertyRule = new DiagnosticDescriptor(
			PreferDirectMemberAccessOverLinqInAnExpressionDiagnosticID,
			"Prefer direct member access over linq in an expression.",
			"Don't use the {0} extension method on a source of type '{1}', use the '{2}' property instead.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Don't use linq extension methods when there is a better alternative.");

		public static readonly DiagnosticDescriptor PreferDirectMemberAccessOverLinqInAnExpression_UseIndexerRule = new DiagnosticDescriptor(
			PreferDirectMemberAccessOverLinqInAnExpressionDiagnosticID,
			"Prefer direct member access over linq in an expression.",
			"Don't use the {0} extension method on a source of type '{1}', use the indexer instead.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Don't use linq extension methods when there is a better alternative.");

		public static readonly DiagnosticDescriptor PreferArrayEmptyOverNewArrayConstructionRule = new DiagnosticDescriptor(
			PreferArrayEmptyOverNewArrayConstructionDiagnosticID,
			"Prefer Array.Empty<T>() over creating a new empty array.",
			"Prefer to use Array.Empty<T>() instead of creating a new empty array.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Array.Empty<T>() caches the array internally, so you can typically use a pre-existing immutable object instead of creating a new one.");

		public static readonly DiagnosticDescriptor DontCallToStringOnAStringRule = new DiagnosticDescriptor(
			DontCallToStringOnAStringDiagnosticID,
			"Don't call ToString() on a string.",
			"Don't call ToString() on a string.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Calling ToString() on a string object is redundant, just use the original string object.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor PreferNameofOverCallingToStringOnAnEnumRule = new DiagnosticDescriptor(
			PreferNameofOverCallingToStringOnAnEnumDiagnosticID,
			"Prefer nameof over calling ToString on an enum literal.",
			"Prefer nameof over calling ToString on an enum literal.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Prefer nameof over calling ToString on an enum literal.");

		public static readonly DiagnosticDescriptor RemovePointlessOverridesRule = new DiagnosticDescriptor(
			RemovePointlessOverridesDiagnosticID,
			"Overrides should not simply call base.",
			"This member overrides a member in a base class, but does not change the behaviour of the base implementation.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This override doesn't change the behaviour of the base implementation and so should be removed.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RemovePointlessOverrides_MethodRule = new DiagnosticDescriptor(
			RemovePointlessOverridesDiagnosticID,
			"Overrides should not simply call base.",
			"This method overrides a method in a base class, but does not change the behaviour of the base implementation.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This override doesn't change the behaviour of the base implementation and so should be removed.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RemovePointlessOverrides_PropertyRule = new DiagnosticDescriptor(
			RemovePointlessOverridesDiagnosticID,
			"Overrides should not simply call base.",
			"This property overrides a property in a base class, but does not change the behaviour of the base implementation.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This override doesn't change the behaviour of the base implementation and so should be removed.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RemovePointlessOverrides_IndexerRule = new DiagnosticDescriptor(
			RemovePointlessOverridesDiagnosticID,
			"Overrides should not simply call base.",
			"This indexer overrides an indexer in a base class, but does not change the behaviour of the base implementation.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This override doesn't change the behaviour of the base implementation and so should be removed.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RemovePointlessOverrides_EventRule = new DiagnosticDescriptor(
			RemovePointlessOverridesDiagnosticID,
			"Overrides should not simply call base.",
			"This event overrides an event in a base class, but does not change the behaviour of the base implementation.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This override doesn't change the behaviour of the base implementation and so should be removed.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DontEquateValueTypesWithNullRule = new DiagnosticDescriptor(
			DontEquateValueTypesWithNullDiagnosticID,
			"Do not compare value types with null.",
			"Do not compare value types with null.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Value types can never be null. This expression is constant.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor PreferCompletedTaskRule = new DiagnosticDescriptor(
			PreferCompletedTaskDiagnosticID,
			"Prefer Task.CompletedTask when applicable.",
			"Prefer Task.CompletedTask when applicable.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Prefer Task.CompletedTask when applicable.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DontAwaitTriviallyCompletedTasksRule = new DiagnosticDescriptor(
			DontAwaitTriviallyCompletedTasksDiagnosticID,
			"Don't await a trivially completed task.",
			"The task is trivially completed and so there is no point awaiting it.",
			DecruftificationCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "You can skip the task altogether.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor DoNotNestRegionsRule = new DiagnosticDescriptor(
			DoNotNestRegionsDiagnosticID,
			"Do not nest regions.",
			"Do not nest regions.",
			MaintainabilityCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Regions tend to obscure the code and nesting them generally indicates that either the code is poorly structured or it is trying to do too much.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor RegionsShouldNotSplitStructuresRule = new DiagnosticDescriptor(
			RegionsShouldNotSplitStructuresDiagnosticID,
			"Regions should not split structures.",
			"If either the start or end of a declaration/statement/expression is within a region, then both ends should be within the same region.",
			MaintainabilityCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "This region is clearly confused about what it's for. Remove it.",
			customTags: new[]
			{
				WellKnownDiagnosticTags.Unnecessary,
			});

		public static readonly DiagnosticDescriptor ConditionalCompilationDirectivesShouldNotSplitStructuresRule = new DiagnosticDescriptor(
			ConditionalCompilationDirectivesShouldNotSplitStructuresDiagnosticID,
			"Conditional compilation directives should not split structures.",
			"If either the start or end of a declaration/statement/expression is within a conditional compilation block, then both ends should be within the same block.",
			MaintainabilityCategory,
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "The conditional compilation directive is either confused or you are trying to do something dodgy. Changing method signatures/visibility based on compiler directives is just asking for trouble.");

		/// <summary>
		/// Our convention is to omit the 'private' modifier where it is already the default.
		/// </summary>
		public static Diagnostic CreateDoNotUseThePrivateKeywordDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotUseThePrivateKeywordRule, location);
		}

		/// <summary>
		/// The compiler is able to correctly identify which type to use here, so replace the explicit type with var.
		/// </summary>
		public static Diagnostic CreateUseVarWherePossibleDiagnostic(Location location)
		{
			return Diagnostic.Create(UseVarWherePossibleRule, location);
		}

		/// <summary>
		/// You have meaningless whitespace on the end of the line, remove it.
		/// </summary>
		public static Diagnostic CreateDoNotLeaveWhitespaceOnTheEndOfTheLineDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotLeaveWhitespaceOnTheEndOfTheLineRule, location);
		}

		/// <summary>
		/// Our coding convention is to use tabs, not spaces, you may need to fix your settings.
		/// </summary>
		public static Diagnostic CreateIndentWithTabsRatherThanSpacesDiagnostic(Location location)
		{
			return Diagnostic.Create(IndentWithTabsRatherThanSpacesRule, location);
		}

		/// <summary>
		/// All line endings should be using CRLF, this issue usually occures when copying code from another source.
		/// </summary>
		public static Diagnostic CreateUseConsistentLineEndingsDiagnostic(Location location)
		{
			return Diagnostic.Create(UseConsistentLineEndingsRule, location);
		}

		/// <summary>
		/// Our convention is to omit the 'internal' modifier on types where it is already the default.
		/// </summary>
		public static Diagnostic CreateDoNotUseTheInternalKeywordForTopLevelTypesDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotUseTheInternalKeywordForTopLevelTypesRule, location);
		}

		/// <summary>
		/// Do not compare bool to a constant value.
		/// </summary>
		public static Diagnostic CreateDoNotCompareBoolToAConstantValueDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotCompareBoolToAConstantValueRule, location);
		}

		/// <summary>
		/// Do not compare bool to a constant value in an expression.
		/// </summary>
		public static Diagnostic CreateDoNotCompareBoolToAConstantValueInAnExpressionDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotCompareBoolToAConstantValueInAnExpressionRule, location);
		}

		/// <summary>
		/// 'using' directives should be placed before 'using static' directives, which in turn must be placed before 'using' alias directives.
		/// </summary>
		public static Diagnostic CreateUsingDirectivesMustBeOrderedByKindDiagnostic(Location location)
		{
			return Diagnostic.Create(UsingDirectivesMustBeOrderedByKindRule, location);
		}

		/// <summary>
		/// The compiler is able to correctly identify which type to use here, so replace the explicit type with var.
		/// </summary>
		public static Diagnostic CreateUseOutVarWherePossibleDiagnostic(Location location)
		{
			return Diagnostic.Create(UseOutVarWherePossibleRule, location);
		}

		/// <summary>
		/// Only declare 'var' once when deconstructing an object, if applicable.
		/// </summary>
		public static Diagnostic CreateDeconstructWithSingleVarDiagnostic(Location location)
		{
			return Diagnostic.Create(DeconstructWithSingleVarRule, location);
		}

		/// <summary>
		/// The compiler is able to correctly identify which type to use here, so replace the explicit type with var.
		/// </summary>
		public static Diagnostic CreateDeconstructWithVarDiagnostic(Location location)
		{
			return Diagnostic.Create(DeconstructWithVarRule, location);
		}

		/// <summary>
		/// Tuple types don't impart useful semantic information, public interfaces should use a dedicated type instead.
		/// </summary>
		public static Diagnostic CreateAvoidTupleTypesInPublicInterfacesDiagnostic(Location location)
		{
			return Diagnostic.Create(AvoidTupleTypesInPublicInterfacesRule, location);
		}

		/// <summary>
		/// Nesting conditional operators makes the code harder to read, use an 'if' statement instead.
		/// </summary>
		public static Diagnostic CreateDontNestConditionalOperatorsDiagnostic(Location location)
		{
			return Diagnostic.Create(DontNestConditionalOperatorsRule, location);
		}

		/// <summary>
		/// The true operand of a conditional operator should occupy a single line, the same line as the '?' symbol.
		/// </summary>
		public static Diagnostic CreateConditionalOperatorsShouldNotHaveMultilineValues_WhenTrueDiagnostic(Location location)
		{
			return Diagnostic.Create(ConditionalOperatorsShouldNotHaveMultilineValues_WhenTrueRule, location);
		}

		/// <summary>
		/// The false operand of a conditional operator should occupy a single line, the same line as the ':' symbol.
		/// </summary>
		public static Diagnostic CreateConditionalOperatorsShouldNotHaveMultilineValues_WhenFalseDiagnostic(Location location)
		{
			return Diagnostic.Create(ConditionalOperatorsShouldNotHaveMultilineValues_WhenFalseRule, location);
		}

		/// <summary>
		/// Prefer an if-throw over assigning a coalesce-throw expression to the discard symbol.
		/// </summary>
		public static Diagnostic CreateAvoidDiscardCoalesceThrowDiagnostic(Location location)
		{
			return Diagnostic.Create(AvoidDiscardCoalesceThrowRule, location);
		}

		/// <summary>
		/// ConfigureAwait(false) may result in the async method resuming on a non-deterministic thread, and if an exception is then thrown, it will likely be unhandled and result in process termination.
		/// </summary>
		public static Diagnostic CreateDoNotConfigureAwaitFromAsyncVoidDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotConfigureAwaitFromAsyncVoidRule, location);
		}

		/// <summary>
		/// Avoid referencing DEBUG in #if or #elif.
		/// </summary>
		public static Diagnostic CreateAvoidConditionalCompilationBasedOnDebugDiagnostic(Location location)
		{
			return Diagnostic.Create(AvoidConditionalCompilationBasedOnDebugRule, location);
		}

		/// <summary>
		/// This member does not specify an explicit value.
		/// </summary>
		public static Diagnostic CreateFlagEnumsShouldSpecifyExplicitValuesDiagnostic(Location location)
		{
			return Diagnostic.Create(FlagEnumsShouldSpecifyExplicitValuesRule, location);
		}

		/// <summary>
		/// This project does not use Code Contracts.
		/// </summary>
		public static Diagnostic CreateDoNotUseCodeContractsDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotUseCodeContractsRule, location);
		}

		/// <summary>
		/// Encountered a code analysis suppression for the non-existent {targetKind} '{targetName}'. Remove or update it.
		/// </summary>
		public static Diagnostic CreateRemovedOrphanedSuppressionsDiagnostic(Location location, object targetKind, object targetName)
		{
			return Diagnostic.Create(RemovedOrphanedSuppressionsRule, location, targetKind, targetName);
		}

		/// <summary>
		/// Don't use the {extensionName} extension method on a source of type '{sourceTypeName}', use the '{propertyName}' property instead.
		/// </summary>
		public static Diagnostic CreatePreferDirectMemberAccessOverLinq_UsePropertyDiagnostic(Location location, object extensionName, object sourceTypeName, object propertyName)
		{
			return Diagnostic.Create(PreferDirectMemberAccessOverLinq_UsePropertyRule, location, extensionName, sourceTypeName, propertyName);
		}

		/// <summary>
		/// Don't use the {extensionName} extension method on a source of type '{sourceTypeName}', use the indexer instead.
		/// </summary>
		public static Diagnostic CreatePreferDirectMemberAccessOverLinq_UseIndexerDiagnostic(Location location, object extensionName, object sourceTypeName)
		{
			return Diagnostic.Create(PreferDirectMemberAccessOverLinq_UseIndexerRule, location, extensionName, sourceTypeName);
		}

		/// <summary>
		/// Don't use the {extensionName} extension method on a source of type '{sourceTypeName}', use the '{propertyName}' property instead.
		/// </summary>
		public static Diagnostic CreatePreferDirectMemberAccessOverLinqInAnExpression_UsePropertyDiagnostic(Location location, object extensionName, object sourceTypeName, object propertyName)
		{
			return Diagnostic.Create(PreferDirectMemberAccessOverLinqInAnExpression_UsePropertyRule, location, extensionName, sourceTypeName, propertyName);
		}

		/// <summary>
		/// Don't use the {extensionName} extension method on a source of type '{sourceTypeName}', use the indexer instead.
		/// </summary>
		public static Diagnostic CreatePreferDirectMemberAccessOverLinqInAnExpression_UseIndexerDiagnostic(Location location, object extensionName, object sourceTypeName)
		{
			return Diagnostic.Create(PreferDirectMemberAccessOverLinqInAnExpression_UseIndexerRule, location, extensionName, sourceTypeName);
		}

		/// <summary>
		/// Prefer to use Array.Empty<T>() instead of creating a new empty array.
		/// </summary>
		public static Diagnostic CreatePreferArrayEmptyOverNewArrayConstructionDiagnostic(Location location)
		{
			return Diagnostic.Create(PreferArrayEmptyOverNewArrayConstructionRule, location);
		}

		/// <summary>
		/// Don't call ToString() on a string.
		/// </summary>
		public static Diagnostic CreateDontCallToStringOnAStringDiagnostic(Location location)
		{
			return Diagnostic.Create(DontCallToStringOnAStringRule, location);
		}

		/// <summary>
		/// Prefer nameof over calling ToString on an enum literal.
		/// </summary>
		public static Diagnostic CreatePreferNameofOverCallingToStringOnAnEnumDiagnostic(Location location)
		{
			return Diagnostic.Create(PreferNameofOverCallingToStringOnAnEnumRule, location);
		}

		/// <summary>
		/// This member overrides a member in a base class, but does not change the behaviour of the base implementation.
		/// </summary>
		public static Diagnostic CreateRemovePointlessOverridesDiagnostic(Location location)
		{
			return Diagnostic.Create(RemovePointlessOverridesRule, location);
		}

		/// <summary>
		/// This method overrides a method in a base class, but does not change the behaviour of the base implementation.
		/// </summary>
		public static Diagnostic CreateRemovePointlessOverrides_MethodDiagnostic(Location location)
		{
			return Diagnostic.Create(RemovePointlessOverrides_MethodRule, location);
		}

		/// <summary>
		/// This property overrides a property in a base class, but does not change the behaviour of the base implementation.
		/// </summary>
		public static Diagnostic CreateRemovePointlessOverrides_PropertyDiagnostic(Location location)
		{
			return Diagnostic.Create(RemovePointlessOverrides_PropertyRule, location);
		}

		/// <summary>
		/// This indexer overrides an indexer in a base class, but does not change the behaviour of the base implementation.
		/// </summary>
		public static Diagnostic CreateRemovePointlessOverrides_IndexerDiagnostic(Location location)
		{
			return Diagnostic.Create(RemovePointlessOverrides_IndexerRule, location);
		}

		/// <summary>
		/// This event overrides an event in a base class, but does not change the behaviour of the base implementation.
		/// </summary>
		public static Diagnostic CreateRemovePointlessOverrides_EventDiagnostic(Location location)
		{
			return Diagnostic.Create(RemovePointlessOverrides_EventRule, location);
		}

		/// <summary>
		/// Do not compare value types with null.
		/// </summary>
		public static Diagnostic CreateDontEquateValueTypesWithNullDiagnostic(Location location)
		{
			return Diagnostic.Create(DontEquateValueTypesWithNullRule, location);
		}

		/// <summary>
		/// Prefer Task.CompletedTask when applicable.
		/// </summary>
		public static Diagnostic CreatePreferCompletedTaskDiagnostic(Location location)
		{
			return Diagnostic.Create(PreferCompletedTaskRule, location);
		}

		/// <summary>
		/// The task is trivially completed and so there is no point awaiting it.
		/// </summary>
		public static Diagnostic CreateDontAwaitTriviallyCompletedTasksDiagnostic(Location location)
		{
			return Diagnostic.Create(DontAwaitTriviallyCompletedTasksRule, location);
		}

		/// <summary>
		/// Do not nest regions.
		/// </summary>
		public static Diagnostic CreateDoNotNestRegionsDiagnostic(Location location)
		{
			return Diagnostic.Create(DoNotNestRegionsRule, location);
		}

		/// <summary>
		/// If either the start or end of a declaration/statement/expression is within a region, then both ends should be within the same region.
		/// </summary>
		public static Diagnostic CreateRegionsShouldNotSplitStructuresDiagnostic(Location location)
		{
			return Diagnostic.Create(RegionsShouldNotSplitStructuresRule, location);
		}

		/// <summary>
		/// If either the start or end of a declaration/statement/expression is within a conditional compilation block, then both ends should be within the same block.
		/// </summary>
		public static Diagnostic CreateConditionalCompilationDirectivesShouldNotSplitStructuresDiagnostic(Location location)
		{
			return Diagnostic.Create(ConditionalCompilationDirectivesShouldNotSplitStructuresRule, location);
		}
	}
}
