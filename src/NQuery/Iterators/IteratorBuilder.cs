﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Iterators
{
    internal sealed class IteratorBuilder
    {
        private readonly Stack<RowBufferAllocation> _outerRowBufferAllocations = new Stack<RowBufferAllocation>(); 

        public static Iterator Build(BoundRelation relation)
        {
            var builder = new IteratorBuilder();
            return builder.BuildRelation(relation);
        }

        private IEnumerable<RowBufferAllocation> BuildRowBufferAllocations(BoundRelation input, RowBuffer inputRowBuffer)
        {
            var inputAllocation = BuildRowBufferAllocation(input, inputRowBuffer);
            return BuildRowBufferAllocations(inputAllocation);
        }

        private IEnumerable<RowBufferAllocation> BuildRowBufferAllocations(RowBufferAllocation input)
        {
            return ImmutableArray.Create(input).Concat(_outerRowBufferAllocations);
        }

        private IEnumerable<RowBufferAllocation> BuildRowBufferAllocations(RowBufferAllocation left, RowBufferAllocation right)
        {
            return ImmutableArray.Create(left, right).Concat(_outerRowBufferAllocations);
        }

        private static RowBufferAllocation BuildRowBufferAllocation(BoundRelation input, RowBuffer rowBuffer)
        {
            return new RowBufferAllocation(rowBuffer, input.GetOutputValues());
        }

        private static IteratorFunction BuildFunction(BoundExpression expression, IEnumerable<RowBufferAllocation> rowBufferAllocations)
        {
            return ExpressionBuilder.BuildIteratorFunction(expression, rowBufferAllocations);
        }

        private static IteratorPredicate BuildPredicate(BoundExpression predicate, IEnumerable<RowBufferAllocation> rowBufferAllocations)
        {
            return ExpressionBuilder.BuildIteratorPredicate(predicate, rowBufferAllocations);
        }

        private Iterator BuildRelation(BoundRelation relation)
        {
            switch (relation.Kind)
            {
                case BoundNodeKind.ConstantRelation:
                    return BuildConstant();
                case BoundNodeKind.TableRelation:
                    return BuildTable((BoundTableRelation)relation);
                case BoundNodeKind.JoinRelation:
                    return BuildJoin((BoundJoinRelation)relation);
                case BoundNodeKind.HashMatchRelation:
                    return BuildHashMatch((BoundHashMatchRelation)relation);
                case BoundNodeKind.FilterRelation:
                    return BuildFilter((BoundFilterRelation)relation);
                case BoundNodeKind.ComputeRelation:
                    return BuildCompute((BoundComputeRelation)relation);
                case BoundNodeKind.TopRelation:
                    return BuildTop((BoundTopRelation)relation);
                case BoundNodeKind.SortRelation:
                    return BuildSort((BoundSortRelation)relation);
                case BoundNodeKind.ConcatenationRelation:
                    return BuildConcatenationRelation((BoundConcatenationRelation)relation);
                case BoundNodeKind.StreamAggregatesRelation:
                    return BuildStreamAggregatesRelation((BoundStreamAggregatesRelation)relation);
                case BoundNodeKind.ProjectRelation:
                    return BuildProject((BoundProjectRelation)relation);
                default:
                    throw new ArgumentOutOfRangeException(nameof(relation), $"Unknown relation kind: {relation.Kind}.");
            }
        }

        private static Iterator BuildConstant()
        {
            return new ConstantIterator();
        }

        private static Iterator BuildTable(BoundTableRelation relation)
        {
            var schemaTableSymbol = (SchemaTableSymbol) relation.TableInstance.Table;
            var tableDefinition = schemaTableSymbol.Definition;
            // TODO: We proably want to only define the needed columns here.
            var columnInstances = relation.TableInstance.ColumnInstances;
            var definedValues = columnInstances.Select(ci => ci.Column)
                                               .Cast<SchemaColumnSymbol>()
                                               .Select(c => c.Definition)
                                               .ToImmutableArray();
            return new TableIterator(tableDefinition, definedValues);
        }

        private Iterator BuildJoin(BoundJoinRelation relation)
        {
            var left = BuildRelation(relation.Left);
            var leftAllocation = BuildRowBufferAllocation(relation.Left, left.RowBuffer);

            var right = BuildRelation(relation.Right);
            var rightAllocation = BuildRowBufferAllocation(relation.Right, right.RowBuffer);

            var rowBufferAllocations = BuildRowBufferAllocations(leftAllocation, rightAllocation);
            var predicate = BuildPredicate(relation.Condition, rowBufferAllocations);

            return new InnerNestedLoopsIterator(left, right, predicate);
        }

        private Iterator BuildHashMatch(BoundHashMatchRelation relation)
        {
            var build = BuildRelation(relation.Build);
            var buildAllocation = BuildRowBufferAllocation(relation.Build, build.RowBuffer);
            var buildIndex = buildAllocation[relation.BuildKey];

            var probe = BuildRelation(relation.Probe);
            var probeAllocation = BuildRowBufferAllocation(relation.Probe, probe.RowBuffer);
            var probeIndex = probeAllocation[relation.ProbeKey];

            var rowBufferAllocations = BuildRowBufferAllocations(buildAllocation, probeAllocation);
            var predicate = BuildPredicate(relation.Remainder, rowBufferAllocations);

            return new HashMatchIterator(relation.LogicalOperator, build, probe, buildIndex, probeIndex, predicate);
        }

        private Iterator BuildFilter(BoundFilterRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var rowBufferAllocations = BuildRowBufferAllocations(relation.Input, input.RowBuffer);
            var predicate = BuildPredicate(relation.Condition, rowBufferAllocations);
            return new FilterIterator(input, predicate);
        }

        private Iterator BuildCompute(BoundComputeRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var rowBufferAllocations = BuildRowBufferAllocations(relation.Input, input.RowBuffer);
            var definedValue = relation.DefinedValues
                                       .Select(dv => BuildFunction(dv.Expression, rowBufferAllocations))
                                       .ToImmutableArray();
            return new ComputeScalarIterator(input, definedValue);
        }

        private Iterator BuildTop(BoundTopRelation relation)
        {
            return relation.TieEntries.Any()
                       ? BuildTopWithTies(relation)
                       : BuildTopWithoutTies(relation);
        }

        private Iterator BuildTopWithTies(BoundTopRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var rowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var tieEntries = relation.TieEntries.Select(t => rowBufferAllocation[t.ValueSlot]).ToImmutableArray();
            var tieComparers = relation.TieEntries.Select(t => t.Comparer).ToImmutableArray();
            return new TopWithTiesIterator(input, relation.Limit, tieEntries, tieComparers);
        }

        private Iterator BuildTopWithoutTies(BoundTopRelation relation)
        {
            var input = BuildRelation(relation.Input);
            return new TopIterator(input, relation.Limit);
        }

        private Iterator BuildSort(BoundSortRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputRowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var sortEntries = relation.SortedValues
                                      .Select(v => inputRowBufferAllocation[v.ValueSlot])
                                      .ToImmutableArray();
            var comparers = relation.SortedValues.Select(v => v.Comparer).ToImmutableArray();
            return relation.IsDistinct
                ? new DistinctSortIterator(input, sortEntries, comparers)
                : new SortIterator(input, sortEntries, comparers);
        }

        private Iterator BuildConcatenationRelation(BoundConcatenationRelation relation)
        {
            var inputs = relation.Inputs.Select(BuildRelation);
            var outputValueSlotCount = relation.DefinedValues.Length;
            return new ConcatenationIterator(inputs, outputValueSlotCount);
        }

        private Iterator BuildStreamAggregatesRelation(BoundStreamAggregatesRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputRowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var rowBufferAllocations = BuildRowBufferAllocations(inputRowBufferAllocation);
            var aggregators = relation.Aggregates
                                      .Select(a => a.Aggregatable.CreateAggregator())
                                      .ToImmutableArray();
            var argumentFunctions = relation.Aggregates
                                            .Select(a => BuildFunction(a.Argument, rowBufferAllocations))
                                            .ToImmutableArray();
            var groupEntries = relation.Groups
                                       .Select(v => inputRowBufferAllocation[v])
                                       .ToImmutableArray();
            return new StreamAggregateIterator(input, groupEntries, aggregators, argumentFunctions);
        }

        private Iterator BuildProject(BoundProjectRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputRowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var projectedIndices = relation.Outputs.Select(vs => inputRowBufferAllocation[vs]).ToImmutableArray();
            return new ProjectionIterator(input, projectedIndices);
        }
    }
}