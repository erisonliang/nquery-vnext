using System;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (BoundQuery, ImmutableArray<ColumnSymbol>)> anchorBinder
        )
            : this(name, anchorBinder, s => ImmutableArray<BoundQuery>.Empty)
        {
        }

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (BoundQuery, ImmutableArray<ColumnSymbol>)> anchorBinder,
            Func<CommonTableExpressionSymbol, ImmutableArray<BoundQuery>> recursiveBinder
        )
            : base(name)
        {
            var tuple = anchorBinder(this);
            Anchor = tuple.Item1;
            Columns = tuple.Item2;
            RecursiveMembers = recursiveBinder(this);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.CommonTableExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }

        internal BoundQuery Anchor { get; }

        internal ImmutableArray<BoundQuery> RecursiveMembers { get; }
    }
}