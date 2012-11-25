using System;

namespace NQuery
{
    public enum DiagnosticId
    {
        InternalError,

        IllegalInputCharacter,
        UnterminatedComment,
        UnterminatedString,
        UnterminatedQuotedIdentifier,
        UnterminatedParenthesizedIdentifier,
        UnterminatedDate,

        InvalidDate,
        InvalidInteger,
        InvalidReal,
        InvalidBinary,
        InvalidOctal,
        InvalidHex,
        InvalidTypeReference,
        NumberTooLarge,
        TokenExpected,
        SimpleExpressionExpected,
        TableReferenceExpected,
        InvalidOperatorForAllAny,

        UndeclaredTable,
        UndeclaredTableInstance,
        UndeclaredVariable,
        UndeclaredFunction,
        UndeclaredAggregate,
        UndeclaredMethod,
        UndeclaredColumn,
        UndeclaredProperty,
        UndeclaredType,
        ColumnTableOrVariableNotDeclared,
        AmbiguousReference,
        AmbiguousTableRef,
        AmbiguousColumnRef,
        AmbiguousTable,
        AmbiguousVariable,
        AmbiguousAggregate,
        AmbiguousProperty,
        AmbiguousType,
        AmbiguousInvocation,
        InvocationRequiresParenthesis,
        CannotApplyUnaryOperator,
        AmbiguousUnaryOperator,
        CannotApplyBinaryOperator,
        AmbiguousBinaryOperator,
        AmbiguousConversion,
        AsteriskModifierNotAllowed,
        WhenMustEvaluateToBool,
        CannotLoadTypeAssembly,
        CannotFoldConstants,
        CannotConvert,

        MustSpecifyTableToSelectFrom,
        AggregateCannotContainAggregate,
        AggregateCannotContainSubquery,
        AggregateDoesNotSupportType,
        AggregateInWhere,
        AggregateInOn,
        AggregateInGroupBy,
        AggregateContainsColumnsFromDifferentQueries,
        AggregateInvalidInCurrentContext,
        DuplicateTableRefInFrom,
        TableRefInaccessible,
        TopWithTiesRequiresOrderBy,
        OrderByColumnPositionIsOutOfRange,
        WhereClauseMustEvaluateToBool,
        OnClauseMustEvaluateToBool,
        HavingClauseMustEvaluateToBool,
        SelectExpressionNotAggregatedAndNoGroupBy,
        SelectExpressionNotAggregatedOrGrouped,
        HavingExpressionNotAggregatedOrGrouped,
        OrderByExpressionNotAggregatedAndNoGroupBy,
        OrderByExpressionNotAggregatedOrGrouped,
        OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified,
        InvalidDataTypeInSelectDistinct,
        InvalidDataTypeInGroupBy,
        InvalidDataTypeInOrderBy,
        InvalidDataTypeInUnion,
        DifferentExpressionCountInBinaryQuery,
        OrderByItemsMustBeInSelectListIfUnionSpecified,
        OrderByItemsMustBeInSelectListIfDistinctSpecified,
        GroupByItemDoesNotReferenceAnyColumns,
        ConstantExpressionInOrderBy,
        TooManyExpressionsInSelectListOfSubquery,
        InvalidRowReference,
        NoColumnAliasSpecified,
        CteHasMoreColumnsThanSpecified,
        CteHasFewerColumnsThanSpecified,
        CteHasDuplicateColumnName,
        CteHasDuplicateTableName,
        CteDoesNotHaveUnionAll,
        CteDoesNotHaveAnchorMember,
        CteContainsRecursiveReferenceInSubquery,
        CteContainsUnexpectedAnchorMember,
        CteContainsMultipleRecursiveReferences,
        CteContainsUnion,
        CteContainsDistinct,
        CteContainsTop,
        CteContainsOuterJoin,
        CteContainsGroupByHavingOrAggregate,
        CteHasTypeMismatchBetweenAnchorAndRecursivePart
    }
}